import warnings
warnings.simplefilter("ignore", UserWarning)
import numpy as np
olderr = np.seterr(all='ignore')

import os
from scipy import misc, ndimage

# from utils import checkDirAndCreate
from depth2HeightMskHelper import *
from labelHelper import *

import layers_builder as layers
from python_utils import utils
from python_utils.preprocessing import preprocess_img

import tensorflow as tf
from keras import backend as K
from keras.models import model_from_json, load_model
from keras.utils.generic_utils import CustomObjectScope

class depth2maskTester(object):
    """docstring fordepth2maskTester."""
    def __init__(self, rootpath, srcImgPath, modelFilePath = None):
        self.depthHelper = depth2HeightMskHelper()
        # self.classifier = pspClassifier(rootpath, srcImgPath, modelFile = modelFilePath)
        # self.labelHelper = labelHelper(classifier = self.classifier)
    def fit(self,depthAddr = None, rawDepthAddr = None, camAddr=None, labelFile = None, imgName = None, forwardMethod = False):
        self.depthHelper.fit(depthAddr,rawDepthAddr,camAddr,forwardMethod=forwardMethod)
        if(forwardMethod and self.depthHelper.detectedBoxes == 0):
            return None
        self.labelHelper.fit(self.depthHelper, labelName = imgName, forwardMethod = forwardMethod)
    def save(self, obstacleName, heigtMapName):
        self.labelHelper.writeObstacles2File(obstacleName)
        # saveOpencvMatrix(heigtMapName, self.depthHelper.heightMap)
class pspClassifierKeras(object):
    def __init__(self, weightPath, nb_classes = 150, resnet_layers=50, input_shape=(473, 473)):
        self.input_shape = input_shape
        json_path = weightPath + ".json"
        h5_path =  weightPath +  ".h5"
        with CustomObjectScope({'Interp': layers.Interp}):
            with open(json_path, 'r') as file_handle:
                self.model = model_from_json(file_handle.read())
        self.model.load_weights(h5_path)

    def predict(self, img, flip_evaluation=False):
        """
        Predict segementation for an image.

        Arguments:
            img: must be rowsxcolsx3
        """
        h_ori, w_ori = img.shape[:2]

        # Preprocess
        img = misc.imresize(img, self.input_shape)


        # These are the means for the ImageNet pretrained ResNet
        DATA_MEAN = np.array([[[123.68, 116.779, 103.939]]])  # RGB order

        img = img - DATA_MEAN
        img = img[:, :, ::-1]  # RGB => BGR
        img = img.astype('float32')
        print("Predicting...")

        probs = self.feed_forward(img, flip_evaluation)

        if img.shape[0:1] != self.input_shape:  # upscale prediction if necessary
            h, w = probs.shape[:2]
            probs = ndimage.zoom(probs, (1. * h_ori / h, 1. * w_ori / w, 1.),
                                 order=1, prefilter=False)

        print("Finished prediction...")

        return probs
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

if __name__ == "__main__":
    rootpath = '../../InputData/'
    weightPath= rootpath + "weights/pspnet50_ade20k"
    resForInputFile = rootpath + "intermediate/fixedObj.txt"
    resForHeightMap = rootpath + "intermediate/heightMapData.yml"
    srcImgPath = rootpath+'imgs/'
    srcImgName = "2483.jpg"

    tf_config = tf.ConfigProto()
    tf_config.gpu_options.allow_growth = True
    os.environ["CUDA_VISIBLE_DEVICES"] = "0"

    sess = tf.Session()
    K.set_session(sess)

    with sess.as_default():

        classifier = pspClassifierKeras(weightPath)

        img = misc.imread(srcImgPath + srcImgName, mode='RGB')
        probs = classifier.predict(img)
        cm = np.argmax(probs, axis=2)
        misc.imsave('outputTest.png', cm)
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
