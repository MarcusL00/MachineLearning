
from unittest import case
from app import app
from flask import request, abort

from app.utilities.cleanCSV import cleanCSVData
from app.ML_models.linearRegressionModel import main

@app.route("/prediction", methods=["POST"])

def make_prediction():
    data = request
    csv_file = data.files.get("csv_file")
    cleaned = cleanCSVData(csv_file)
    main(cleaned)

    model_type = data.form.get("model_type")

    if model_type == "linear_regression":
        from app.ML_models.linearRegressionModel import train_and_plot, prepareData, handleCSV
    elif model_type == "decision_tree":
        from app.ML_models.decisionTreeModel import train_and_plot, prepareData, handleCSV
    elif model_type == "logistic_regression":
        from app.ML_models.logisticRegressionModel import train_and_plot, prepareData, handle
    else:
        raise ValueError(f"Unknown model type: {model_type}")


    
