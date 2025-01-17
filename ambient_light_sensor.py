import os
import base64
from apds9960 import APDS9960
import RPi.GPIO as GPIO
import smbus
import time
import math
import json
from enum import Enum
import requests


class Brightness(Enum):
    NONE = 0
    LOW = 10
    MEDIUM = 50
    HIGH = 85


def post_brightness(brightness: Brightness, base_url, encoded_api_key):
    try:
        url = f"{base_url}/matrix/brightness"

        payload = {
            "Brightness": brightness.value,
            "Source": "LightSensor"
        }

        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Basic {encoded_api_key}"
        }

        response = requests.post(url, headers=headers, json=payload, timeout=3)

        if response.status_code == 200:
            print(f"Successfully posted [Brightness: {brightness.value}]")
        else:
            print("Post fail")
    except Exception:
        print("Failed to connect to server")


print("Reading server information from matrix_settings")

data_folder_path = os.path.join(os.getcwd(), "Matrix", "Data")
api_key_file_path = os.path.join(data_folder_path, "api_key")
matrix_settings_file_path = os.path.join(data_folder_path, "matrix_settings.json")

with open(api_key_file_path, "r") as file:
    unencoded_key = file.read()

encoded_key = base64.b64encode(unencoded_key.encode('utf-8')).decode('utf-8')

with open(matrix_settings_file_path, "r") as file:
    server_url = json.load(file)["ServerUrl"]


print("Setting up APDS9960 sensor")

port = 1
bus = smbus.SMBus(port)

apds = APDS9960(bus)

GPIO.setmode(GPIO.BOARD)
GPIO.setup(7, GPIO.IN)

apds.enableLightSensor()


print("Starting loop")

try:
    previous_brightness = Brightness.NONE
    current_brightness = Brightness.LOW
    last_val = -100

    while True:
        time.sleep(0.25)
        ambientLight = apds.readAmbientLight()

        tolerance = 25 if last_val > 75 else 2

        if not math.isclose(last_val, ambientLight, abs_tol=tolerance):
            last_val = ambientLight

            previous_brightness = current_brightness

            if ambientLight >= 270:
                current_brightness = Brightness.HIGH
            elif ambientLight < 3:
                current_brightness = Brightness.LOW
            else:
                current_brightness = Brightness.MEDIUM

            if previous_brightness != current_brightness:
                post_brightness(current_brightness, server_url, encoded_key)

            print(f"Ambient Light: {ambientLight}, Brightness = {current_brightness.value}")
finally:
    GPIO.cleanup()
