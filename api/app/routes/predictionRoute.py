
from app import app
from flask import request, abort

from app.utilities.cleanCSV import cleanCSVData

@app.route("/prediction", methods=["POST"])

def make_prediction():
    data = request
    csv_file = data.files.get("csv_file")
    
    cleaned = cleanCSVData(csv_file)

    



