import pandas as pd


def cleanCSVData(csv_file):
    """Read an uploaded CSV file (file-like) and perform simple cleaning.

    Returns a JSON-serializable representation (list of records).
    """
    # pandas can read file-like objects provided by Flask's FileStorage
    df = pd.read_csv(csv_file)

    data_copy = df.copy()

    # drop columns with more than 30% missing values
    threshold = len(df) * 0.3
    data_copy = data_copy.dropna(axis=1, thresh=len(data_copy) - threshold)

    # Return records (list of dicts) so it's JSON-serializable
    return data_copy.to_dict(orient="records")