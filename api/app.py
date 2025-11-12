
def create_app():
    from flask import Flask
    from flask_cors import CORS

    app = Flask(__name__)
    CORS(app)

    # Register blueprints
    from app.routes.predictionRoute import make_prediction
    app.register_blueprint(make_prediction, url_prefix="/api")

    return app

app = create_app()

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
