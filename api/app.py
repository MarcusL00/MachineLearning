
def create_app():
    from flask import Flask
    from flask_cors import CORS
    
    app = Flask(__name__)
    CORS(app)

    # Register blueprints
    from api.app.routes.predictionRoute import prediction_bp
    app.register_blueprint(prediction_bp, url_prefix="/api")

    return app

app = create_app()

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
