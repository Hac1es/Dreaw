from paddleocr import PaddleOCR
#Khởi tạo ocr
ocr = PaddleOCR(lang="vi", use_angle_cls=True, det_db_box_thresh=0.2, det_db_unclip_ratio=2.0, use_gpu=True, score_threshold=0, rec_batch_num=12);

def calculate_canvas_size(img_width, img_height, max_width=70):
    """
    Tính toán kích thước canvas dựa trên kích thước ảnh và giới hạn.
    """
    # Giả định chiều rộng mong muốn
    canvas_width = min(max_width, img_width // 20)  # Tùy chỉnh độ phân giải xuống
    canvas_height = round(canvas_width * (img_height / img_width))
    
    return canvas_width, canvas_height

def map_to_canvas(x, y, img_width, img_height, canvas_width, canvas_height):
    """
    Tạo ra tọa độ chữ canvas giả lập dựa trên tọa độ ảnh
    """
    canvas_x = int(round(x / img_width * (canvas_width - 1)))
    canvas_y = int(round(y / img_height * (canvas_height - 1)))
    return canvas_x, canvas_y

def draw_text_on_canvas_optimized(data, img_width, img_height, canvas_width, canvas_height):
    """
    Tạo ra một chuỗi giả lập lại bức ảnh
    """
    canvas = {}

    for sublist in data:
        for item in sublist:
            bbox = item[0]
            text = item[1][0]
            center_x = sum([point[0] for point in bbox]) / len(bbox)
            center_y = sum([point[1] for point in bbox]) / len(bbox)
            canvas_x, canvas_y = map_to_canvas(center_x, center_y, img_width, img_height, canvas_width, canvas_height)

            # Kiểm tra nếu dòng Y chưa có trong canvas
            if canvas_y not in canvas:
                canvas[canvas_y] = [" " for _ in range(canvas_width)]
            
            line = canvas[canvas_y]
            text_start = max(0, canvas_x - len(text) // 2)
            text_end = min(canvas_width, text_start + len(text))

            # Kiểm tra và xử lý va chạm
            collision = False
            while not collision:
                collision = True
                for i in range(text_start, min(text_end, len(line))):
                    if line[i] != " ":
                        # Nếu có va chạm, dịch sang phải một ký tự
                        collision = False
                        text_start += 1
                        text_end = min(canvas_width, text_start + len(text))
                        break

                # Nếu không có không gian trống, bỏ qua văn bản này
                if text_start >= canvas_width:
                    break

            # Vẽ văn bản sau khi xử lý va chạm
            if collision:
                for i, char in enumerate(text[:text_end - text_start]):
                    line[text_start + i] = char

    # Chuyển từ điển thành danh sách các dòng, giữ nguyên thứ tự
    sorted_keys = sorted(canvas.keys())
    filtered_canvas = [canvas[key] for key in sorted_keys]

    return filtered_canvas

def process(img):
    data = ocr.ocr(img)
    if not data or data[0] is None:
        return (False, None)
    # Vẽ canvas ASCII
    img_height, img_width = img.shape[:2]
    canvas_width, canvas_height = calculate_canvas_size(img_width, img_height)
    canvas = draw_text_on_canvas_optimized(data, img_width, img_height, canvas_width, canvas_height)

    # Chuyển canvas thành chuỗi ASCII
    ascii_art = "\n".join("".join(line) for line in canvas)
    return (True, ascii_art)
    
