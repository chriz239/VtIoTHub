import spidev  # spi dev module from https://github.com/doceme/py-spidev


spi = spidev.SpiDev()
spi.open(0, 0)
print "spi CLK: " + str(spi.max_speed_hz / 1000) + "khz"  # print clock speed in khz
spi.max_speed_hz = 20 * 1000  # set spi clk to 100 khz
print "Set spi CLK: " + str(spi.max_speed_hz / 1000) + "khz"


def read_ad_channel(ch=None):
    if ch is None:
        ch = 0

    selection = (8 + ch) << 4
    val = spi.xfer2([1, selection, 0])  # first element is the preamble
    # second element(byte) contains the channel and the MSB of the return Value
    # the third Byte contains the lower byte of the result
    data = ((val[1] & 3) << 8) | val[2]  # in val[1] are bit 9 & bit 8 (MSB 's) in val[2] are the rest bit 7 downto 0
    return ~data


if __name__ == "__main__":
    delay = 0.5
    import time
    try:
        while True:
            value = read_ad_channel(0)
            print "Channel " + str(0) + ":" + str(value)
            time.sleep(delay)
    except KeyboardInterrupt:
        print "Exiting"
        spi.close()

