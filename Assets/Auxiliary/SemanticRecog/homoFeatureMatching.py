import numpy as np
import cv2
from fishyEye import undistort
from matplotlib import pyplot as plt


rootpath = '../../InputData/'
matrixPath = rootpath + 'Matrices/'

DIM = np.load(matrixPath + 'DIM.npy')
D = np.load(matrixPath + 'D.npy')
K = np.load(matrixPath + 'K.npy')
M = np.load(matrixPath + 'homoMat.npy')

rgb = np.load('calibrate_img/test/rgb0.npy')
gray = 0.2126 * rgb[:,:,0] + 0.7152 * rgb[:,:,1] + 0.0722*rgb[:,:,2]
img1 = (gray * 255).astype(np.uint8)

img2 = np.load("calibrate_img/test/infrared4.npy")
img2 = undistort(img2, DIM, K, D)
#
# np.save('img1', img1)
# np.save('img2', img2)
#
# cv2.imshow('rgb',img1)
# cv2.imshow('depth', img2)
# cv2.waitKey(0)
# cv2.destroyAllWindows()
# cv2.waitKey(1)

MIN_MATCH_COUNT = 10
# Initiate SIFT detector
sift = cv2.xfeatures2d.SIFT_create()
# find the keypoints and descriptors with SIFT
kp1, des1 = sift.detectAndCompute(img1,None)
kp2, des2 = sift.detectAndCompute(img2,None)


f1 = cv2.drawKeypoints(img1,kp1,None,(255,0,0),4)
cv2.imshow('res', f1)
cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.waitKey(1)
#
f2 = cv2.drawKeypoints(img2,kp2,None,(255,0,0),4)
cv2.imshow('res2', f2)
cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.waitKey(1)

h,w = img1.shape
pts = np.float32([ [0,0],[0,h-1],[w-1,h-1],[w-1,0] ]).reshape(-1,1,2)
dst = cv2.perspectiveTransform(pts,M)
for cor in dst:
    # print(cor.shape)
    img2 = cv2.circle(img2,tuple(cor[0]),5,(255,0,0),4)
cv2.imshow('res3', img2)

cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.waitKey(1)
