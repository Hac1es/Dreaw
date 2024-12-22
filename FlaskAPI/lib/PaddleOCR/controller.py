
import cv2
from flask import request, Blueprint, Response
import numpy as np
from .services import process

ocr = Blueprint('ocr', __name__)

@ocr.route("/ocr", methods=["POST"])
def ocr_this_image():
    # Kiểm tra file ảnh được gửi
    if 'file' not in request.files:
        return Response(status=400)
    file = request.files['file']
    if file.filename == '':
        return Response(status=400)
    
    # Đọc file ảnh
    img = cv2.imdecode(np.frombuffer(file.read(), np.uint8), cv2.IMREAD_COLOR)
    if img is None:
        return Response(status=400)
    
    #Xử lý file ảnh
    flag, result = process(img)
    if flag == False:
        return Response(status=422)
    return Response(result, status=200, content_type="text/plain")