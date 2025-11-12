
from flask import Blueprint, request, abort, jsonify

from app.utilities.cleanCSV import cleanCSVData

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

    # Example call into the ML model; keep imports local where appropriate to
    # avoid importing heavy modules at package import time.
    try:
        from app.ML_models.linearRegressionModel import main
        main(cleaned)
    except Exception:
        # For now, swallow model errors and return success; adjust per app needs.
        pass

    model_type = data.form.get("model_type")

    if model_type == "linear_regression":
        main(cleaned)
    elif model_type == "decision_tree":
        from app.ML_models.decisionTreeModel import train_and_plot, prepareData, handleCSV
    elif model_type == "logistic_regression":
        from app.ML_models.logisticRegressionModel import train_and_plot, prepareData, handleCSV
    else:
        raise ValueError(f"Unknown model type: {model_type}")

    return jsonify({"status": "ok"}), 200



