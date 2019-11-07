/*
Advamation C Library -- JNI interface

:Version:   2014-02-21
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: ADVABOARD_DONE.java,v 1.2 2014/05/02 11:49:25 rk Exp $
***/

package am_rpi;

public class ADVABOARD_DONE {
    public final static int POWER	= 0x0030;
    public final static int PINS	= 0x1000;
    public final static int RTC		= 0x0020;
    public final static int IO_DIGITAL	= 0x1000;
    public final static int IO_ANALOG	= 0x2000;
    public final static int IO_PWM	= 0x2000;
    public final static int EVENTPWR	= 0x0008;
    public final static int EVENTIO	= 0x0800;
    public final static int I2CCLKEXT	= 0x0101;
    public final static int RS485CFG	= 0x1100;
    public final static int LED		= 0x0800;
};

