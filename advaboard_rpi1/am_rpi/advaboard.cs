/*
Library for AdvaBoard RPi1 -- JNI interface

:Version:   2014-03-06
:SeeAlso:   am_rpi_advaboard.c/.h

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: advaboard.java,v 1.9 2014/05/02 11:49:25 rk Exp $
*/

package am_rpi;
import am_rpi.AM_ERROR_Exception;

public class advaboard {
    static {
        System.loadLibrary("am_rpi_advaboard-jni");
    }

    //----------------------------------------
    // library init/exit/logging
    public static synchronized native void init()		throws AM_ERROR_Exception;
    public static synchronized native void exit();
    public static synchronized native void logging(int level, String logfile, int logfile_level)  throws AM_ERROR_Exception;

    //----------------------------------------
    // basic / low-level access to the AdvaBoard RPi1
    public static synchronized native int[] f912_comm(int cmd, int[] txdata, int rxlen)	throws AM_ERROR_Exception;
    public static synchronized native int   f912_cmdstatus()		throws AM_ERROR_Exception;
    public static synchronized native void  f912_lock()			throws AM_ERROR_Exception;
    public static synchronized native void  f912_lock(int timeout_ms)	throws AM_ERROR_Exception;
    public static synchronized native void  f912_unlock();
    public static synchronized native void  f912_locking_timeout(int timeout_ms);
    public static synchronized native void  f912_fakelock();

    public static synchronized native int[] f353_comm(int cmd, int[] txdata, int rxlen)	throws AM_ERROR_Exception;
    public static synchronized native int   f353_cmdstatus()		throws AM_ERROR_Exception;
    public static synchronized native void  f353_lock()			throws AM_ERROR_Exception;
    public static synchronized native void  f353_lock(int timeout_ms)	throws AM_ERROR_Exception;
    public static synchronized native void  f353_unlock();
    public static synchronized native void  f353_locking_timeout(int timeout_ms);
    public static synchronized native void  f353_fakelock();

    public static synchronized native int[] cpldreg(int[] tx)		throws AM_ERROR_Exception;

    public static synchronized native void  gpio_write(long gpios, boolean level);
    public static synchronized native long  gpio_read( long gpios);

    public static synchronized native int[] spi_comm(int[] txdata)	throws AM_ERROR_Exception;
    public static synchronized native void  spi_clk(int clkdiv)		throws AM_ERROR_Exception;
    public static synchronized native void  spi_clkcfg(int clkcfg)	throws AM_ERROR_Exception;
    public static synchronized native void  spi_mux(int mux)		throws AM_ERROR_Exception;
    public static synchronized native void  spi_lock()			throws AM_ERROR_Exception;
    public static synchronized native void  spi_lock(int timeout_ms)	throws AM_ERROR_Exception;
    public static synchronized native void  spi_unlock();
    public static synchronized native void  spi_locking_timeout(int timeout_ms);
    public static synchronized native void  spi_fakelock();

    public static synchronized native int[] rpii2c_comm(int address, int[] txdata, int rxlen)  throws AM_ERROR_Exception;
    public static synchronized native void  rpii2c_clk(int clkdiv)	throws AM_ERROR_Exception;
    public static synchronized native void  rpii2c_lock()		throws AM_ERROR_Exception;
    public static synchronized native void  rpii2c_lock(int timeout_ms)	throws AM_ERROR_Exception;
    public static synchronized native void  rpii2c_unlock();
    public static synchronized native void  rpii2c_locking_timeout(int timeout_ms);
    public static synchronized native void  rpii2c_fakelock();

    //----------------------------------------
    // system information
    public static synchronized native String[] info()			throws AM_ERROR_Exception;
    public static synchronized native float[]  temperature_read()	throws AM_ERROR_Exception;

    //----------------------------------------
    // status
    public static synchronized native boolean done(int cmd)		throws AM_ERROR_Exception;

    //----------------------------------------
    // Power-Management
    public static synchronized native float[]   voltages_read()			throws AM_ERROR_Exception;
    public static synchronized native float[]   voltages_read(int select)	throws AM_ERROR_Exception;
    public static synchronized native void      power(int pwron, int cpld_3V, int peripheral_5V, int rpi, int tft, int tft_backlight)  throws AM_ERROR_Exception;
    public static synchronized native boolean[] power_read()				throws AM_ERROR_Exception;
    public static synchronized native void      pins(int espi_rs485, int rpi, int tft)	throws AM_ERROR_Exception;
    public static synchronized native boolean[] pins_read()				throws AM_ERROR_Exception;
    public static synchronized native void      power_tftblpwm(int pwm)			throws AM_ERROR_Exception;
    public static synchronized native int       power_tftblpwm_read()			throws AM_ERROR_Exception;

    //----------------------------------------
    // RTC
    public static synchronized native int  rtc_status()			throws AM_ERROR_Exception;
    public static synchronized native void rtc_control(int control)	throws AM_ERROR_Exception;

    public static synchronized native long[] rtc_read()			throws AM_ERROR_Exception;
    public static synchronized native void   rtc_write(long time)	throws AM_ERROR_Exception;
    public static synchronized native void   rtc_set(  long time)	throws AM_ERROR_Exception;

    //----------------------------------------
    // I/O
    public static synchronized native void    io0_config(int io4_ida0, int io5_ida1, int io6_pwm0, int io7_pwm1)	throws AM_ERROR_Exception;
    public static synchronized native int[]   io0_config_read()			throws AM_ERROR_Exception;
    public static synchronized native int[]   io0_config_read(int select)	throws AM_ERROR_Exception;
    public static synchronized native void    io1_config(int cpldio0, int cpldio1, int cpldio2, int cpldio3, int cpldio4, int cpldio5, int cpldio6, int cpldio7)	throws AM_ERROR_Exception;
    public static synchronized native int[]   io1_config_read()			throws AM_ERROR_Exception;
    public static synchronized native int[]   io1_config_read(int select)	throws AM_ERROR_Exception;
    public static synchronized native int[]   io_status(int len)		throws AM_ERROR_Exception;
    public static synchronized native void    io_control(int control)		throws AM_ERROR_Exception;
    public static synchronized native void    io_write(int io4567, int cpldio01234567)	throws AM_ERROR_Exception;
    public static synchronized native int     io_read1()			throws AM_ERROR_Exception;
    public static synchronized native int[]   io_read2()			throws AM_ERROR_Exception;
    public static synchronized native void    io_set(int i)			throws AM_ERROR_Exception;
    public static synchronized native void    io_clr(int i)			throws AM_ERROR_Exception;
    public static synchronized native boolean io_get(int i)			throws AM_ERROR_Exception;

    public static synchronized native int[] analog_status(int len)		throws AM_ERROR_Exception;
    public static synchronized native void  analog_control(long control)	throws AM_ERROR_Exception;
    public static synchronized native int[] analogins_read()			throws AM_ERROR_Exception;
    public static synchronized native int[] analogins_read(int select)		throws AM_ERROR_Exception;
    public static synchronized native int   analogin_read(int i)		throws AM_ERROR_Exception;
    public static synchronized native void  analogouts_write(int ida0, int ida0_scale, int ida1, int ida1_scale)	throws AM_ERROR_Exception;
    public static synchronized native int[] analogouts_read()			throws AM_ERROR_Exception;
    public static synchronized native int[] analogouts_read(int select)	throws AM_ERROR_Exception;
    public static synchronized native void  analogout_write(int i, int value, int scale)	throws AM_ERROR_Exception;
    public static synchronized native int   analogout_read( int i)				throws AM_ERROR_Exception;
    public static synchronized native void  pwm_clk(int clkdiv, int prescaler)	throws AM_ERROR_Exception;
    public static synchronized native int[] pwm_clk_read()			throws AM_ERROR_Exception;
    public static synchronized native int[] pwm_clk_read(int select)		throws AM_ERROR_Exception;
    public static synchronized native void  pwm_write(int i, int value)		throws AM_ERROR_Exception;
    public static synchronized native int   pwm_read( int i)			throws AM_ERROR_Exception;
    public static synchronized native int[] pwms_read()				throws AM_ERROR_Exception;
    public static synchronized native int[] pwms_read(int select)		throws AM_ERROR_Exception;

    //----------------------------------------
    // event-based automation
    public static synchronized native int   eventpwr_flags()				throws AM_ERROR_Exception;
    public static synchronized native void  eventpwr_clear(int flags)			throws AM_ERROR_Exception;
    public static synchronized native void  eventpwr_trigger(int handler_pwr)		throws AM_ERROR_Exception;
    public static synchronized native void  eventpwr_output_write(int handler_pwr, int mask0, int output0)	throws AM_ERROR_Exception;
    public static synchronized native int[] eventpwr_output_read( int handler_pwr)				throws AM_ERROR_Exception;
    public static synchronized native void  eventpwr_analogcontrol_write(int handler_pwr, int analogcontrol0, int analogcontrol1, int analogcontrol2)	throws AM_ERROR_Exception;
    public static synchronized native int[] eventpwr_analogcontrol_read( int handler_pwr)								throws AM_ERROR_Exception;
    public static synchronized native void  eventpwr_power_write(int handler_pwr, int pwron, int cpld_3V, int periphery_5V, int rpi, int tft, int tft_backlight)	throws AM_ERROR_Exception;
    public static synchronized native int[] eventpwr_power_read( int handler_pwr)									throws AM_ERROR_Exception;

    public static synchronized native void   eventpwr_i2cint_write(int handler_pwr_en)	throws AM_ERROR_Exception;
    public static synchronized native int    eventpwr_i2cint_read()			throws AM_ERROR_Exception;
    public static synchronized native void   rtc_alarm_write(int i, int handler_pwr, long time, long periodic)	throws AM_ERROR_Exception;
    public static synchronized native void   rtc_alarm_set(  int i, int handler_pwr, long time, long periodic)	throws AM_ERROR_Exception;
    public static synchronized native long[] rtc_alarm_read( int i)						throws AM_ERROR_Exception;

    public static synchronized native int   eventio_flags()			throws AM_ERROR_Exception;
    public static synchronized native void  eventio_clear(int flags)		throws AM_ERROR_Exception;
    public static synchronized native void  eventio_trigger(int handler_io)	throws AM_ERROR_Exception;
    public static synchronized native void  eventio_output_write(int  handler_io, int mask0, int output0, int mask1, int output1, int mask2, int output2)	throws AM_ERROR_Exception;
    public static synchronized native int[] eventio_output_read( int  handler_io)										throws AM_ERROR_Exception;
    public static synchronized native void  eventio_control_write(int handler_io, int control)							throws AM_ERROR_Exception;
    public static synchronized native int   eventio_control_read( int handler_io)								throws AM_ERROR_Exception;
    public static synchronized native void  eventio_trigger_write(int event_io, int handler_io, int mask0, int input0, int mask1, int input1)	throws AM_ERROR_Exception;
    public static synchronized native int[] eventio_trigger_read( int event_io)									throws AM_ERROR_Exception;

    //----------------------------------------
    // EEPROM
    public static synchronized native void f912_store()	throws AM_ERROR_Exception;
    public static synchronized native void f353_store()	throws AM_ERROR_Exception;

    //----------------------------------------
    // E-SPI
    public static synchronized native int[]   espi(int sel, int[] txdata)	throws AM_ERROR_Exception;
    public static synchronized native void    espi_clk(int clkdiv, int clkcfg)	throws AM_ERROR_Exception;
    public static synchronized native void    espi_sel(int sel)			throws AM_ERROR_Exception;
    public static synchronized native int     espi_sel_read();
    public static synchronized native boolean espi_int_read()			throws AM_ERROR_Exception;
    public static synchronized native void    espi_lock()			throws AM_ERROR_Exception;
    public static synchronized native void    espi_lock(int timeout_ms)		throws AM_ERROR_Exception;
    public static synchronized native void    espi_unlock();
    public static synchronized native void    espi_locking_timeout(int timeout_ms);
    public static synchronized native void    espi_fakelock();

    //----------------------------------------
    // I2C
    public static synchronized native int[]   i2c(int address, int[] txdata, int rxlen)		throws AM_ERROR_Exception;
    public static synchronized native int[]   i2c_prot(int address, int cmd)			throws AM_ERROR_Exception;
    public static synchronized native int[]   i2c_prot(int address, int cmd, int[] txdata)	throws AM_ERROR_Exception;
    public static synchronized native void    i2c_clk(int clkdiv)	throws AM_ERROR_Exception;
    public static synchronized native boolean i2c_int_read();
    public static synchronized native void    i2c_f912int_clear()	throws AM_ERROR_Exception;
    public static synchronized native void    i2c_lock()		throws AM_ERROR_Exception;
    public static synchronized native void    i2c_lock(int timeout_ms)	throws AM_ERROR_Exception;
    public static synchronized native void    i2c_unlock();
    public static synchronized native void    i2c_locking_timeout(int timeout_ms);
    public static synchronized native void    i2c_fakelock();

    // I2C clock-extension / clock-extension-injection
    public static synchronized native void  i2c_clkext(int delay)	throws AM_ERROR_Exception;
    public static synchronized native int[] i2c_clkext_read()		throws AM_ERROR_Exception;
    public static synchronized native void  i2c_clkextinj(int addressmask, int address)	throws AM_ERROR_Exception;
    public static synchronized native int[] i2c_clkextinj_read()	throws AM_ERROR_Exception;

    //----------------------------------------
    // RS485/RS232
    public static synchronized native void  rs485_cfg(int mux, int speed, int bit9)	throws AM_ERROR_Exception;
    public static synchronized native int[] rs485_cfg_read()				throws AM_ERROR_Exception;

    //----------------------------------------
    // misc
    public static synchronized native void led(int mode)	throws AM_ERROR_Exception;
}

