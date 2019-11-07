/*
Advamation C Library -- JNI interface

:Version:   2014-02-05
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: AM_ERROR_CODE.java,v 1.4 2014/05/02 11:49:25 rk Exp $
***/

package am_rpi;

public class AM_ERROR_CODE {
    public final static int SUCCESS                = 0;
    public final static int INVALID_ARGUMENT       = -22;
    public final static int IO                     = -5;
    public final static int PERMISSIONS            = -13;
    public final static int MEMORY                 = -100;

    public final static int NOT_INITIALIZED        = -200;

    public final static int TIMEOUT                = -0x0101;
    public final static int ABORTED                = -0x0102;
    public final static int OPERATION_NOT_SUPPORTED= -0x0111;
    public final static int OPERATION_NOT_PERMITTED= -0x0112;
    public final static int NOT_IMPLEMENTED        = -0x0120;

    public final static int TEMPORARY              = -0x0200;
    public final static int   BUSY                 = -0x0201;
    public final static int   BUFFER               = -0x0210;
    public final static int     BUFFER_EMPTY       = -0x0211;
    public final static int     BUFFER_FULL        = -0x0212;

    public final static int DATA                   = -0x0300;
    public final static int   DATA_TOOMUCH         = -0x0301;
    public final static int   DATA_NOTENOUGH       = -0x0302;
    public final static int   DATA_INVALIDRANGE    = -0x0303;
    public final static int   DATA_INVALID         = -0x0304;

    public final static int FILE                   = -0x0400;
    public final static int   FILE_OPEN            = -0x0410;
    public final static int   FILE_READ            = -0x0420;
    public final static int   FILE_WRITE           = -0x0430;

    public final static int DEVICE                 = -0x0500;
    public final static int   NO_DEVICE            = -0x0501;
    public final static int   INVALID_DEVICE       = -0x0502;
    public final static int   DEVICE_NOT_SUPPORTED = -0x0503;
    public final static int   DEVICE_NOT_INITIALIZED=-0x0504;
    public final static int   DEVICE_NOT_READY     = -0x0505;
    public final static int   DEVICE_UNEXPECTED    = -0x0506;
    public final static int   DEVICE_TIMEOUT       = -0x0507;
    public final static int   DEVICE_LOCKED        = -0x0508;

    public final static int COMM_FAILED            = -0x0600;
    public final static int   COMM_REFUSED         = -0x0601;
    public final static int   COMM_ABORTED         = -0x0602;
    public final static int   COMM_TIMEOUT         = -0x0603;
    public final static int   COMM_NOACK           = -0x0604;
    public final static int   COMM_CRC             = -0x0605;
    public final static int   COMM_TOOLARGE        = -0x0606;
    public final static int   COMM_TOOSHORT        = -0x0607;
    public final static int   COMM_INVALID         = -0x0608;
    public final static int   COMM_SEND            = -0x0609;

    public final static int CONFIGFILE                 = -0x1000;
    public final static int   CONFIGFILE_READ          = -0x1001;
    public final static int   CONFIGFILE_WRITE         = -0x1002;
    public final static int   CONFIGFILE_NO_GROUP      = -0x1003;
    public final static int   CONFIGFILE_UNKNOWN_ENTRY = -0x1004;
}

