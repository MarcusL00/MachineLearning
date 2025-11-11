from linearRegressionModel import train_and_plot
import numpy as np
import pandas as pd
import matplotlib
from sklearn.linear_model import LinearRegression



df = pd.read_csv("examples/test.csv")

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

    plt = matplotlib.pyplot
    plt.scatter(X, y, color='blue', label='Actual Data')
    plt.plot(X, y_pred, color='red', linewidth=2, label='Regression Line')
    plt.xlabel('Features')
    plt.ylabel('Target')
    plt.title('Linear Regression')
    plt.legend()
    plt.savefig(output_path)
    plt.close()
