import numpy as np
import pandas as pd
import matplotlib
# Use a non-interactive backend so plotting works inside headless containers
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from sklearn.linear_model import LinearRegression


def handleCSV(df):
    headers = df.head().to_dict().keys()
    return list(headers)


def prepareData(df, list_of_features, target_variable):
    X = df[list_of_features]
    y = df[target_variable]
    return X, y


def train_and_plot(X, y, output_path):
    model = LinearRegression()
    model.fit(X, y)
    y_pred = model.predict(X)

    plt.scatter(X.values if hasattr(X, "values") else X, y, color='blue', label='Actual Data')
    # If X has multiple columns, plotting a single regression line is more complex;
    # this example assumes a single feature for plotting.
    try:
        plt.plot(X.values, y_pred, color='red', linewidth=2, label='Regression Line')
    except Exception:
        # Fallback: skip plotting the fitted line if shapes are incompatible
        pass
    plt.xlabel('Features')
    plt.ylabel('Target')
    plt.title('Linear Regression')
    plt.legend()
    plt.savefig(output_path)
    plt.close()


def test(csv_data_frame):
    # Expect csv_data_frame to be a pandas DataFrame
    df = csv_data_frame

    # Process the data
    headers = handleCSV(df)
    X, y = prepareData(df, headers[:-1], headers[-1])

    # Train the model and plot
    train_and_plot(X, y, "linear_regression_output.png")