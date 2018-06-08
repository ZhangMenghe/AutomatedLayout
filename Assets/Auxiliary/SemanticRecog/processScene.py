import warnings
warnings.simplefilter("ignore", UserWarning)
import numpy as np
olderr = np.seterr(all='ignore')

import os
from scipy import misc, ndimage
import cv2

from depth2HeightMskHelper import *
from labelHelper import *
from fishyEye import undistort
from camera import getCameraParam
import layers_builder as layers
from python_utils import utils
from python_utils.preprocessing import preprocess_img

import tensorflow as tf
from keras import backend as K
from keras.models import model_from_json, load_model
from keras.utils.generic_utils import CustomObjectScope

class pspClassifierKeras(object):
    def __init__(self, weightPath, nb_classes = 150, resnet_layers=50, input_shape=(473, 473)):
        self.input_shape = input_shape
        json_path = weightPath + ".json"
        h5_path =  weightPath +  ".h5"
        with CustomObjectScope({'Interp': layers.Interp}):
            with open(json_path, 'r') as file_handle:
                self.model = model_from_json(file_handle.read())
        self.model.load_weights(h5_path)
        print("setup classifier")

    def predict(self, img, flip_evaluation=False):
        """
        Predict segementation for an image.

        Arguments:
            img: must be rowsxcolsx3
        """
        print("start predicting...")
        h_ori, w_ori = img.shape[:2]

        # Preprocess
        img = misc.imresize(img, self.input_shape)


        # These are the means for the ImageNet pretrained ResNet
        DATA_MEAN = np.array([[[123.68, 116.779, 103.939]]])  # RGB order

        img = img - DATA_MEAN
        img = img[:, :, ::-1]  # RGB => BGR
        img = img.astype('float32')

        probs = self.feed_forward(img, flip_evaluation)

        if img.shape[0:1] != self.input_shape:  # upscale prediction if necessary
            h, w = probs.shape[:2]
            probs = ndimage.zoom(probs, (1. * h_ori / h, 1. * w_ori / w, 1.),
                                 order=1, prefilter=False)
        self.probs = probs
        self.labels = np.argmax(probs, axis=2)
        print("predicted")
    def feed_forward(self, data, flip_evaluation=False):
        assert data.shape == (self.input_shape[0], self.input_shape[1], 3)

        if flip_evaluation:
            print("Predict flipped")
            input_with_flipped = np.array(
                [data, np.flip(data, axis=1)])
            prediction_with_flipped = self.model.predict(input_with_flipped)
            prediction = (prediction_with_flipped[
                          0] + np.fliplr(prediction_with_flipped[1])) / 2.0
        else:
            prediction = self.model.predict(np.expand_dims(data, 0))[0]
        return prediction
class hololenCamera(object):
    def __init__(self, matrixPath = None, K = None, DIM=None, D=None, cameraMatrix=None):
        if(cameraMatrix is not None):
            self.cameraMatrix = cameraMatrix
        if(matrixPath is not None):
            self.DIM = np.load(matrixPath + 'DIM.npy')
            self.D = np.load(matrixPath + 'D.npy')
            self.K = np.load(matrixPath + 'K.npy')
            self.cameraMatrix = self.K
        else:
            self.DIM = DIM
            self.D = D
            self.K = K
    def preprocessing(self, depth, missingMsk):
        p_depth = undistort(depth, self.DIM, self.K, self.D)
        p_mask = undistort(missingMsk, self.DIM, self.K, self.D)
        p_mask[p_mask<0.1] = 0
        p_mask[p_mask>=0.1] = 1

        beforeRange = np.max(p_depth) - np.min(p_depth)
        p_depth_show = (p_depth.astype(np.float32)/beforeRange * 255).astype(np.uint8)

        p_depth_show = cv2.fastNlMeansDenoising(p_depth_show, None, 7, 7, 21)
        kernel = np.ones((3,3),np.uint8)
        p_depth_show = cv2.dilate(p_depth_show,kernel,iterations = 1)

        p_depth_after = p_depth_show.astype(np.float32)/255 * beforeRange
        p_mask_show = (p_mask * 255).astype(np.uint8)
        p_depth_after = misc.imresize(p_depth_after, (473,473),interp="nearest")
        p_mask = misc.imresize(p_mask, (473,473),interp="nearest")
        return p_depth_after, p_mask
class processScene(object):
    def __init__(self, modelFilePath,cameraHelper, HomoMatrix = None):
        tf_config = tf.ConfigProto()
        tf_config.gpu_options.allow_growth = True
        os.environ["CUDA_VISIBLE_DEVICES"] = "0"
        # session and run
        self.sess = tf.Session()
        K.set_session(self.sess)
        with self.sess.as_default():
            self.classifier = pspClassifierKeras(modelFilePath)
        self.cameraHelper = cameraHelper
        self.depthHelper = depth2HeightMskHelper(cameraHelper)
        self.labelHelper = labelHelper(HomoMatrix)

    def fit(self, img, depth, missingMsk, needPreprocessing = True):
        if(needPreprocessing):
            depth,missingMsk = self.cameraHelper.preprocessing(depth, missingMsk)
        self.depthHelper.fit(depth, missingMsk)
        # labelImg = np.load('data.npy')

        with self.sess.as_default():
            self.classifier.predict(img)
            np.save('data', self.classifier.labels)
            labelImg = self.classifier.labels
            # misc.imsave('outputTest.png', self.classifier.labels)
        self.labelHelper.fit(self.depthHelper, labelImg)
    def save(self, obstacleName, heigtMapName, floorMapName):
        self.labelHelper.writeObstacles2File(obstacleName)
        np.save(heigtMapName, self.depthHelper.heightMap)
        np.save(floorMapName, self.labelHelper.floorMat)


if __name__ == "__main__":
    rootpath = '../../InputData/'
    matrixPath = rootpath + 'Matrices/'
    modelFilePath = rootpath + "weights/pspnet50_ade20k"
    resForInputFile = rootpath + "intermediate/fixedObj.txt"
    resForHeightMap = rootpath + "intermediate/heightMapData"
    resForFloor = rootpath + "intermediate/floorMapData"

    ##################debug input from dataset#################
    # srcImgPath = rootpath+'imgs/'
    # depthAddr = rootpath+'depth/'
    # rawDepthAddr = rootpath+'depth_raw/'
    # srcImgName = "2483"
    # ext = ['.jpg', '.png']
    # img = misc.imread(srcImgPath + srcImgName + ext[0], mode='RGB')
    # depthImage = misc.imread(depthAddr+ srcImgName + ext[1], mode='F').astype(float)/100
    # missingMask = (misc.imread(rawDepthAddr+srcImgName + ext[1], mode='F').astype(float) == 0)
    # cameraHelper = hololenCamera(cameraMatrix = getCameraParam())
    ##################debug from hololens###########################
    srcImgPath = 'calibrate_img/'
    img = (np.load(srcImgPath +'rgb.npy') * 255).astype(np.uint8)
    # img = misc.imresize(img,(427,561))
    depth_smallscope = np.load(srcImgPath +'depth_small.npy')
    depth_largescope = np.load(srcImgPath +'depth_large.npy')
    missingMask = np.load(srcImgPath + 'missingMask_small.npy')
    homoMat = np.load(matrixPath + 'homoMat.npy')
    cameraHelper = hololenCamera(matrixPath = matrixPath)
    ##############################################

    processor = processScene(modelFilePath, cameraHelper, HomoMatrix = homoMat)
    processor.fit(img, depth_smallscope, missingMask)
    processor.save(resForInputFile, resForHeightMap, resForFloor)
    # d2tTester = depth2maskTester(rootpath, srcImgPath, modelFilePath)
    # filenameSet = listdir(srcImgPath)
    # for name in filenameSet:
    #     if(len(name.split('.')) > 2):
    #         continue
    #     pureName = name.split('.')[0]
    #     depthAddr  = rootpath + 'depth/'+pureName+'.png'
    #     rawDepthAddr = rootpath + 'depth_raw/'+pureName+'.png'
    #     d2tTester.fit(depthAddr = depthAddr, rawDepthAddr = rawDepthAddr, imgName = name)
    #     d2tTester.save(resForInputFile, resForHeightMap)
