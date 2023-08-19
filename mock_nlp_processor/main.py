from flask import Flask, jsonify, request

app = Flask(__name__)


@app.route("/processor/process", methods=["POST"])
def process_text():
    try:
        data = request.get_json()
        if "text" in data:
            input_text = data["text"]
            print(f"Processing text: {input_text}")
            processed_response = input_text
            response = {"text": processed_response}
            return jsonify(response), 200
        else:
            return jsonify({"error": "Missing 'text' field in request"}), 400
    except Exception as e:
        return jsonify({"error": str(e)}), 500


if __name__ == "__main__":
    app.run(host="localhost", port=7034, debug=True)
