# ----------------------- Pipline -----------------------------------------
#
# ----------------------------------------------------------------

from pyspark.ml import Pipeline
from pyspark.mllib import classification, feature
from pyspark.sql import SparkSession
import os
import shutil

spark = SparkSession.builder.appName("ml").getOrCreate()

all_results = None

data_directory = "E:/Dataset"
processed_files = []  # List to keep track of processed files
# Get a list of all gzipped JSON files in the directory
json_files = [file for file in os.listdir(data_directory) if file.endswith(".json.gz")]

existing_output_path = os.path.join("output", "final_result_all.json")
if os.path.exists(existing_output_path):
    all_results = spark.read.json(existing_output_path)
else:
    all_results = None

i = 0
j = 0

print(all_results.count())

for file in json_files:
    file_path = os.path.join(data_directory, file)

    try:
        df = spark.read.json(file_path)
    except Exception as e:
        print(f"Error reading file {file_path}: {str(e)}")
        continue

    df.createOrReplaceTempView("github_data")

    result = spark.sql("""
        SELECT DISTINCT repo.name, repo.url
        FROM github_data
        WHERE (type = 'ForkEvent' AND payload.forkee.language = 'C#') OR 
              (type = 'PullRequestEvent' AND payload.pull_request.base.repo.language = 'C#' AND payload.pull_request.base.repo.stargazers_count > 10)
    """)
    processed_files.append(file)  # Add the file to the list of processed files
    df.unpersist()
    spark.catalog.dropTempView("github_data")

    # Show the result
    print(result.count())
    print(file)
    processed_directory = "E:/Processed"

    if all_results is None:
        all_results = result
    else:
        all_results = all_results.union(result)

    i += 1

    if i == 10:
        all_results = all_results.distinct()
        output_path = os.path.join("output", "final_result_all" + str(j) + ".json")
        all_results.write.mode("overwrite").json(output_path)
        print('writing' + file)
        i = 0
        j += 1


all_results = all_results.distinct()
output_path = os.path.join("output", "final_result_all.json")
all_results.write.mode("overwrite").json(output_path)