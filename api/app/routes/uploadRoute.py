from app import app
from flask import request
from flask import abort, app


@app.route("/upload", methods=["POST"])
def upload_file():
    """Handle file upload and return a success message."""
    if 'file' not in request.files:
        abort(400, description="No file part in the request")
    file = request.files['file']
    if file.filename == '':
        abort(400, description="No selected file")
        
    save_path = f"uploads/{file.filename}"
    file.save(save_path)
    return {"message": f"File '{file.filename}' uploaded successfully"}, 200