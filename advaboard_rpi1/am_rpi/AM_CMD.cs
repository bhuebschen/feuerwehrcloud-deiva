/*
Advamation C Library -- JNI interface

:Version:   2014-03-06
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: AM_CMD.java,v 1.7 2014/05/02 11:49:25 rk Exp $
***/

package am_rpi;

public class AM_CMD {
    // communication
    public final static int ADDRESS_GET      = 0x01;
    public final static int ADDRESS_SET      = 0x02;
    public final static int ADDRESS_STORE    = 0x03;
    public final static int SCAN             = 0x04;
    public final static int UID_ADDRESS_GET  = 0x05;
    public final static int UID_ADDRESS_SET  = 0x06;
    public final static int UID              = 0x07;
    public final static int COMMPARAM_READ   = 0x08;
    public final static int COMMPARAM_WRITE  = 0x09;
    public final static int CMDSTATUS        = 0x0A;
    // device-information
    public final static int SERNO            = 0x10;
    public final static int DEVID            = 0x11;
    public final static int DEVINFO          = 0x12;
    // EEPROM
    public final static int EEPROM_STATUS    = 0x18;
    public final static int EEPROM_SIZE      = 0x19;
    public final static int EEPROM_READ      = 0x1A;
    public final static int EEPROM_WRITE     = 0x1B;
    public final static int EEPROM_CFG_LOAD  = 0x1C;
    public final static int EEPROM_CFG_STORE = 0x1D;
    // device
    public final static int ECHO             = 0x20;
    public final static int LED              = 0x21;
    public final static int RESET            = 0x28;
    public final static int HANDLER_FLAGS    = 0x29;
    public final static int HANDLER_CLEAR    = 0x2A;
    public final static int HANDLER_TRIGGER  = 0x2B;
    // digital-I/O
    public final static int INPUT_READ1          = 0x30;
    public final static int INPUT_READ2          = 0x31;
    public final static int INPUT_READ4          = 0x32;
    public final static int INPUT_READ8          = 0x33;
    public final static int INPUT_READ           = 0x34;
    public final static int OUTPUT_READ1         = 0x35;
    public final static int OUTPUT_READ2         = 0x36;
    public final static int OUTPUT_READ4         = 0x37;
    public final static int OUTPUT_READ8         = 0x38;
    public final static int OUTPUT_READ          = 0x39;
    public final static int OUTPUT_WRITEN        = 0x3A;
    public final static int OUTPUT_WRITE         = 0x3B;
    public final static int OUTBIT_SET           = 0x3C;
    public final static int OUTBIT_CLR           = 0x3D;
    public final static int IO_UPDATE            = 0x3E;
    public final static int IO_CFG_READ          = 0x40;
    public final static int IO_CFG_WRITE         = 0x41;
    public final static int IO_STATUS            = 0x42;
    public final static int IO_CONTROL           = 0x43;
    public final static int IO_HANDLER_CONTROL_READ  = 0x44;
    public final static int IO_HANDLER_CONTROL_WRITE = 0x45;
    public final static int IO_HANDLER_OUTPUT_READ   = 0x46;
    public final static int IO_HANDLER_OUTPUT_WRITE  = 0x47;
    public final static int IO_EVENT_READ        = 0x48;
    public final static int IO_EVENT_WRITE       = 0x49;
    // analog-I/O
    public final static int ANALOGIN_READ1       = 0x50;
    public final static int ANALOGIN_READ2       = 0x51;
    public final static int ANALOGIN_READ4       = 0x52;
    public final static int ANALOGIN_READ8       = 0x53;
    public final static int ANALOGIN_READ        = 0x54;
    public final static int ANALOGOUT_READ1      = 0x55;
    public final static int ANALOGOUT_READ2      = 0x56;
    public final static int ANALOGOUT_READ4      = 0x57;
    public final static int ANALOGOUT_READ8      = 0x58;
    public final static int ANALOGOUT_READ       = 0x59;
    public final static int ANALOGOUT_WRITEN     = 0x5A;
    public final static int ANALOGOUT_WRITE      = 0x5B;
    public final static int ANALOG_CFG_READ      = 0x60;
    public final static int ANALOG_CFG_WRITE     = 0x61;
    public final static int ANALOG_STATUS        = 0x62;
    public final static int ANALOG_CONTROL       = 0x63;
    public final static int ANALOG_HANDLER_CONTROL_READ  = 0x64;
    public final static int ANALOG_HANDLER_CONTROL_WRITE = 0x65;
    public final static int ANALOG_HANDLER_OUTPUT_READ   = 0x66;
    public final static int ANALOG_HANDLER_OUTPUT_WRITE  = 0x67;
    public final static int ANALOG_EVENT_READ    = 0x68;
    public final static int ANALOG_EVENT_WRITE   = 0x69;
};

