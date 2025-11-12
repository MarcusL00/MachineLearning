from flask_cors import CORS

def create_app():
    from flask import Flask
    app = Flask(__name__)

    # Register blueprints
    from app.routes.upload import upload_bp
    app.register_blueprint(upload_bp, url_prefix="/api")

    return app

app = create_app()
CORS(app)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=8000, debug=True)
