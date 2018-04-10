import warnings
warnings.simplefilter("ignore", UserWarning)

import numpy as np
olderr = np.seterr(all='ignore')
import cv2
from sys import argv
from os import listdir
from os.path import isfile, join
import imageio
from utils import checkDirAndCreate
from depth2HeightMskHelper import *
from labelHelper import *
from pspClassifier import *

from write2FileHelper import saveOpencvMatrix

class depth2maskTester(object):
    """docstring fordepth2maskTester."""
    def __init__(self, rootpath, srcImgPath, modelFilePath = None):
        self.depthHelper = depth2HeightMskHelper()
        self.classifier = pspClassifier(rootpath, srcImgPath, modelFile = modelFilePath)
        self.labelHelper = labelHelper(classifier = self.classifier)
    def fit(self,depthAddr = None, rawDepthAddr = None, camAddr=None, labelFile = None, imgName = None, forwardMethod = False):
        self.depthHelper.fit(depthAddr,rawDepthAddr,camAddr,forwardMethod=forwardMethod)
        if(forwardMethod and self.depthHelper.detectedBoxes == 0):
            return None
        self.labelHelper.fit(self.depthHelper, labelName = imgName, forwardMethod = forwardMethod)
    def save(self, obstacleName, heigtMapName):
        self.labelHelper.writeObstacles2File(obstacleName)
        saveOpencvMatrix(heigtMapName, self.depthHelper.heightMap)

if __name__ == "__main__":
    rootpath = '../../InputData/'
    modelFilePath = "pspnet_sunrgbd_sun_model.pkl"
    resForInputFile = rootpath + "intermediate/fixedObj.txt"
    resForHeightMap = rootpath + "intermediate/heightMapData.yml"
    srcImgPath = rootpath+'imgs/'
    d2tTester = depth2maskTester(rootpath, srcImgPath, modelFilePath)
    filenameSet = listdir(srcImgPath)
    for name in filenameSet:
        if(len(name.split('.')) > 2):
            continue
        pureName = name.split('.')[0]
        depthAddr  = rootpath + 'depth/'+pureName+'.png'
        rawDepthAddr = rootpath + 'depth_raw/'+pureName+'.png'
        d2tTester.fit(depthAddr = depthAddr, rawDepthAddr = rawDepthAddr, imgName = name)
        d2tTester.save(resForInputFile, resForHeightMap)
