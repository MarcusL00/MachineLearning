import pandas as pd
import matplotlib
from sklearn.linear_model import LogisticRegression
import numpy as np

def handleCSV(df):
    headers = df.head().to_dict().keys()
    return list(headers)

def prepareData(df, list_of_features, target_variable):
    X = df[list_of_features]
    y = df[target_variable]
    return X, y

def train_and_plot(X, y, output_path):
    model = LogisticRegression()
    model.fit(X, y)
    y_pred_prob = model.predict_proba(X)[:, 1]  # probability of class 1

    plt = matplotlib.pyplot
    plt.scatter(X, y, color='blue', label='Actual Data')

    # Sort values for a smooth curve
    sorted_idx = np.argsort(X.values.flatten())
    plt.plot(X.values.flatten()[sorted_idx],
             y_pred_prob[sorted_idx],
             color='red', linewidth=2, label='Logistic Curve')

    plt.xlabel('Features')
    plt.ylabel('Probability of Target=1')
    plt.title('Logistic Regression')
    plt.legend()
    plt.savefig(output_path)
    plt.close()
