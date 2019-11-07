/*
Advamation C Library -- JNI interface

:Version:   2013-12-10
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: AM_LOG_LEVEL.java,v 1.3 2014/05/02 11:49:25 rk Exp $
***/

package am_rpi;

public class AM_LOG_LEVEL {
    public final static int ERROR    = 1 << 2; //always fatal
    public final static int CRITICAL = 1 << 3;
    public final static int WARNING  = 1 << 4;
    public final static int MESSAGE  = 1 << 5;
    public final static int INFO     = 1 << 6;
    public final static int DEBUG    = 1 << 7;
};

