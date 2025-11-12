
from flask import Blueprint, request, abort, jsonify

from app.utilities.cleanCSV import cleanCSVData
from app.ML_models.linearRegressionModel import test
# Expose a Blueprint named `make_prediction` so the application factory
# can import and register it with `app.register_blueprint(make_prediction, ...)`.
make_prediction = Blueprint("make_prediction", __name__)


@make_prediction.route("/prediction", methods=["POST"])
def _handle_make_prediction():
    data = request
    csv_file = data.files.get("csv_file")

    if csv_file is None:
        abort(400, description="csv_file is required")

    cleaned = cleanCSVData(csv_file)
    

    model_type = data.form.get("model_type")

    if model_type == "linear_regression":
        test(cleaned)
    elif model_type == "decision_tree":
        from app.ML_models.decisionTreeModel import train_and_plot, prepareData, handleCSV
    elif model_type == "logistic_regression":
        from app.ML_models.logisticRegressionModel import train_and_plot, prepareData, handleCSV
    else:
        raise ValueError(f"Unknown model type: {model_type}")

    return jsonify({"status": "ok"}), 200



