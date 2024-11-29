from flask import Flask
from .PaddleOCR.controller import ocr

def create_app():
    app = Flask(__name__)
    app.register_blueprint(ocr)
    return app
