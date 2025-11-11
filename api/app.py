from flask import Flask, request, render_template, abort, Response
from jinja2 import TemplateNotFound
from flask_cors import CORS

app = Flask(__name__, template_folder="templates")  # templates/fragments/*.html
CORS(app)

@app.route("/fragment/raw")
def fragment_raw():
    """Return a simple HTML fragment as a string."""
    html = '<div class="fragment"><h2>Raw Header</h2><p>This is an HTML fragment returned directly.</p></div>'
    return Response(html, mimetype="text/html")

@app.route("/fragment/card")
def fragment_card():
    """Return a small dynamic HTML fragment constructed in Python."""
    title = request.args.get("title", "Card Title")
    body = request.args.get("body", "Card body")
    html = f'<div class="card"><h3>{title}</h3><p>{body}</p></div>'
    return Response(html, mimetype="text/html")

@app.route("/fragment/template/<name>")
def fragment_template(name):
    """
    Render and return a template fragment.
    Expects templates/fragments/{name}.html
    Example: GET /fragment/template/header -> templates/fragments/header.html
    """
    try:
        return render_template(f"fragments/{name}.html")
    except TemplateNotFound:
        abort(404, description="Fragment template not found")

if __name__ == "__main__":
    # Run with: python App.py
    app.run(host="0.0.0.0", port=8000, debug=True)
