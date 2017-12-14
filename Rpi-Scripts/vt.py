#!/usr/bin/env python

import random
import time
import sys
from iothub_client import IoTHubClient, IoTHubClientError, IoTHubTransportProvider, IoTHubClientResult
from iothub_client import IoTHubMessage, IoTHubMessageDispositionResult, IoTHubError, DeviceMethodReturnValue
import getSensor

TIMEOUT = 241000
MINIMUM_POLLING_TIME = 9
MESSAGE_TIMEOUT = 10000
RECEIVE_CONTEXT = 0
MESSAGE_COUNT = 0
MESSAGE_SWITCH = True
MESSAGE_TIMESPAN = 3600
ALERTVALUE = 900

#global counters
RECEIVE_CALLBACKS = 0
SEND_CALLBACKS = 0


#ConnectionString
CONNECTION_STRING = "HostName=VtIoTHub.azure-devices.net;DeviceId=raspberrypi;SharedAccessKey=ZSMXQ9PWGnfPIzgIwt8CAz2dWuFKeu5Au96tKo7OpTw="
#Protocol  HTTP OR MQTT OR AMQP
PROTOCOL = IoTHubTransportProvider.AMQP

def iothub_client_init():
    # prepare iothub client
    client = IoTHubClient(CONNECTION_STRING, PROTOCOL)
    client.set_option("product_info", "RaspberryPi-Python")
    client.set_option("timeout", TIMEOUT)
    client.set_option("MinimumPollingTime", MINIMUM_POLLING_TIME)
    # set the time until a message times out
    client.set_option("messageTimeout", MESSAGE_TIMEOUT)
    client.set_message_callback(
        receive_message_callback, RECEIVE_CONTEXT)
    return client

def receive_message_callback(message, counter):
    global RECEIVE_CALLBACKS
    message_buffer = message.get_bytearray()
    size = len(message_buffer)
    print ( "Received Message [%d]:" % counter )
    print ( "    Data: <<<%s>>> & Size=%d" % (message_buffer[:size].decode("utf-8"), size) )
    map_properties = message.properties()
    key_value_pair = map_properties.get_internals()
    print ( "    Properties: %s" % key_value_pair )
    counter += 1
    RECEIVE_CALLBACKS += 1
    print ( "    Total calls received: %d" % RECEIVE_CALLBACKS )
    return IoTHubMessageDispositionResult.ACCEPTED
	
def send_confirmation_callback(message, result, user_context):
    global SEND_CALLBACKS
    print ( "Confirmation[%d] received for message with result = %s" % (user_context, result) )
    map_properties = message.properties()
    print ( "    message_id: %s" % message.message_id )
    print ( "    correlation_id: %s" % message.correlation_id )
    key_value_pair = map_properties.get_internals()
    print ( "    Properties: %s" % key_value_pair )
    SEND_CALLBACKS += 1
    print ( "    Total calls confirmed: %d" % SEND_CALLBACKS )

	
def print_last_message_time(client):
    try:
        last_message = client.get_last_message_receive_time()
        print ( "Last Message: %s" % time.asctime(time.localtime(last_message)) )
        print ( "Actual time : %s" % time.asctime() )
    except IoTHubClientError as iothub_client_error:
        if iothub_client_error.args[0].result == IoTHubClientResult.INDEFINITE_TIME:
            print ( "No message received" )
        else:
            print ( iothub_client_error )
	
def iothub_client_run():
    try:
        client = iothub_client_init()

        if client.protocol == IoTHubTransportProvider.MQTT:
            print ( "IoTHubClient is reporting state" )
            reported_state = "{\"newState\":\"standBy\"}"
            client.send_reported_state(reported_state, len(reported_state), send_reported_state_callback, SEND_REPORTED_STATE_CONTEXT)

        while True:
            global MESSAGE_COUNT,MESSAGE_SWITCH
            if MESSAGE_SWITCH:
                # send a few messages every minute
                print ( "IoTHubClient sending %d messages" % MESSAGE_COUNT )
                #value = random.uniform(0,100) 
                value = getSensor.read_ad_channel() 
                msg_txt_formatted = "{\"Device\":\"Raspberry VT\",\"Humidity\":\"%d\"}" % value
                print (msg_txt_formatted)
                message = IoTHubMessage(msg_txt_formatted)
             
			    # optional: assign ids
                message.message_id = "message_%d" % MESSAGE_COUNT
                message.correlation_id = "correlation_%d" % MESSAGE_COUNT
                # optional: assign properties
                prop_map = message.properties()
                prop_map.add("Alert", "true" if value < ALERTVALUE else "false")
			 
                client.send_event_async(message, send_confirmation_callback, MESSAGE_COUNT)
                print ( "IoTHubClient.send_event_async accepted message [%d] for transmission to IoT Hub." % MESSAGE_COUNT )
                status = client.get_send_status()
                print ( "Send status: %s" % status )
                MESSAGE_COUNT += 1
            time.sleep(MESSAGE_TIMESPAN / 1000.0)

    except IoTHubError as iothub_error:
        print ( "Unexpected error %s from IoTHub" % iothub_error )
        return
    except KeyboardInterrupt:
        print ( "IoTHubClient for VT stopped" )

    print_last_message_time(client)
	
if __name__ == "__main__":
    print ( "\nPython %s" % sys.version )
    print ( "IoT Hub Client for Python" )

    iothub_client_run()

