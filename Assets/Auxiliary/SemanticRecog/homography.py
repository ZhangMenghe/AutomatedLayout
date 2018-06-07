'''
 Based on the following tutorial:
   http://docs.opencv.org/3.0-beta/doc/py_tutorials/py_feature2d/py_feature_homography/py_feature_homography.html
'''

import numpy as np
import cv2
from matplotlib import pyplot as plt
# Load the images in gray scale
# img1 = cv2.imread('box.png', 0)
# img2 = cv2.imread('box_in_scene.png', 0)

rgb = np.load('calibrate_img/rgb.npy')

img2 = np.load('calibrate_img/depth2.npy')

img1 = np.zeros(rgb.shape, dtype = np.uint8)

gray = 0.2126 * rgb[:,:,0] + 0.7152 * rgb[:,:,1] + 0.0722*rgb[:,:,2]

img1 = (gray * 255).astype(np.uint8)
# img1 = img1[int(0.05*img1.shape[0]):int(0.95*img1.shape[0]), int(0.15*img1.shape[1]):int(0.9 * img1.shape[1])]
# img2 = img2[int(0.1*img2.shape[0]):int(0.5*img2.shape[0]), int(0.25*img2.shape[1]):int(0.75 * img2.shape[1])]

np.save('img1', img1)
np.save('img2', img2)
# print(rgb)
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

ret, corners = cv2.findChessboardCorners(img1, (6,9),None)
# print(corners)
f1 = cv2.drawChessboardCorners(img1, (6,9), corners,ret)

ret2,corners2 = cv2.findChessboardCorners(img2, (6,9), None)
f2 = cv2.drawChessboardCorners(img2, (6,9), corners2,ret2)


# # f1 = cv2.drawKeypoints(img1,kp1,None,(255,0,0),4)
# cv2.imshow('res', f1)
# cv2.waitKey(0)
# cv2.destroyAllWindows()
# cv2.waitKey(1)
# #
# # f2 = cv2.drawKeypoints(img2,kp2,None,(255,0,0),4)
# cv2.imshow('res2', f2)
# cv2.waitKey(0)
# cv2.destroyAllWindows()
# cv2.waitKey(1)


FLANN_INDEX_KDTREE = 0
index_params = dict(algorithm = FLANN_INDEX_KDTREE, trees = 5)
search_params = dict(checks = 50)


# BFMatcher with default params
bf = cv2.BFMatcher()
matches = bf.knnMatch(des1,des2, k=2)


# flann = cv2.FlannBasedMatcher(index_params, search_params)

# matches = flann.knnMatch(des1,des2,k=2)

# store all the good matches as per Lowe's ratio test.
good = []
for m,n in matches:
    if m.distance < 0.7*n.distance:
        good.append(m)
if len(good)>MIN_MATCH_COUNT:
    src_pts = np.float32([ kp1[m.queryIdx].pt for m in good ]).reshape(-1,1,2)
    dst_pts = np.float32([ kp2[m.trainIdx].pt for m in good ]).reshape(-1,1,2)

    M, mask = cv2.findHomography(corners, corners2, cv2.RANSAC,5.0)
    print(M)
    matchesMask = mask.ravel().tolist()

    h,w = img1.shape
    # pts = np.float32([ [0,0],[0,h-1],[w-1,h-1],[w-1,0] ]).reshape(-1,1,2)
    # pts = np.float32([ [321.5, 149.],[336.,599.],[1038.6904, 595.15686],[1060., 154.5] ]).reshape(-1,1,2)
    pts = corners.reshape(-1,1,2)
    dst = cv2.perspectiveTransform(pts,M)

    # img2 = cv2.polylines(img2,[np.int32(dst)],True,255,3, cv2.LINE_AA)
    for cor in dst:
        # print(cor.shape)
        img2 = cv2.circle(img2,tuple(cor[0]),5,(255,0,0),4)

    cv2.imshow('res3', img2)
    cv2.waitKey(0)
    cv2.destroyAllWindows()
    cv2.waitKey(1)

else:
    # print "Not enough matches are found - %d/%d" % (len(good),MIN_MATCH_COUNT)
    matchesMask = None

draw_params = dict(matchColor = (0,255,0), # draw matches in green color
                   singlePointColor = None,
                   matchesMask = matchesMask, # draw only inliers
                   flags = 2)

# img3 = cv2.drawMatches(img1,kp1,img2,kp2,good,None,**draw_params)

# plt.imshow(img3, 'gray'),plt.show()
