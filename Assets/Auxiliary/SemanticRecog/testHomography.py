
import numpy as np
import cv2
from matplotlib import pyplot as plt
# Load the images in gray scale
# img1 = cv2.imread('box.png', 0)
# img2 = cv2.imread('box_in_scene.png', 0)

rgb = np.load('calibrate_img/rgb2.npy')

img2 = np.load('calibrate_img/depth_22.npy')

img1 = np.zeros(rgb.shape, dtype = np.uint8)

gray = 0.2126 * rgb[:,:,0] + 0.7152 * rgb[:,:,1] + 0.0722*rgb[:,:,2]

img1 = (gray * 255).astype(np.uint8)


ret, corners = cv2.findChessboardCorners(img1, (6,9),None)

f1 = cv2.drawChessboardCorners(img1, (6,9), corners,ret)
cv2.imshow('res', f1)
cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.waitKey(1)

pts = corners.reshape(-1,1,2)
M = np.load('homoMat.npy')
dst = cv2.perspectiveTransform(pts,M)
for cor in dst:
    # print(cor.shape)
    img2 = cv2.circle(img2,tuple(cor[0]),5,(255,0,0),4)

cv2.imshow('res3', img2)

cv2.waitKey(0)
cv2.destroyAllWindows()
cv2.waitKey(1)
