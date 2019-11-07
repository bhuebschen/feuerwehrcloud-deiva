/*
Advamation C Library -- JNI interface

:Version:   2014-01-22
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: advamation.java,v 1.4 2014/05/02 11:49:25 rk Exp $
***/

namespace am_rpi {

public class advamation {
    static {
        System.loadLibrary("advamation-jni");
    }

    // misc.
    public static native int crc_smbus(int crc, int data);

    // UI
    public static native String error2str(int err);

    // simple integer-array-creator / shortcut
    public static int[] I(int... elems) {
	return elems;
    }
}

    }