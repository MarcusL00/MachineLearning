
from app import app
from flask import request
from flask import abort, app

from app.utilities.cleanCSV import cleanCSVData
@app.route("/prediction", methods=["POST"])

def make_prediction():
    import app.utilities
    data = request
    csv_file = data.files.get("csv_file")
    
    cleaned = cleanCSVData(csv_file)

    



