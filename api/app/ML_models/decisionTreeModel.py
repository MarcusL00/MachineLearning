import pandas as pd
import matplotlib
from sklearn.tree import DecisionTreeClassifier, plot_tree
import matplotlib.pyplot as plt

def handleCSV(df):
    headers = df.head().to_dict().keys()
    return list(headers)

def prepareData(df, list_of_features, target_variable):
    X = df[list_of_features]
    y = df[target_variable]
    return X, y

def train_and_plot(X, y, output_path):
    # Train a decision tree classifier
    model = DecisionTreeClassifier(max_depth=3, random_state=42)
    model.fit(X, y)

    # Plot the tree structure
    plt.figure(figsize=(12, 8))
    plot_tree(model, feature_names=X.columns, class_names=[str(c) for c in set(y)], filled=True)
    plt.title("Decision Tree Classifier")
    plt.savefig(output_path)
    plt.close()

def main():
    # Load your data
    df = pd.read_csv("your_data.csv")

    # Process the data
    headers = handleCSV(df)
    X, y = prepareData(df, headers[:-1], headers[-1])

    # Train the model and plot
    train_and_plot(X, y, "decision_tree_output.png")
if __name__ == "__main__":
    main()