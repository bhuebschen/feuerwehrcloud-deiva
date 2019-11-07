/*
Advamation C Library -- JNI interface

Exception class.

:Version:   2014-01-22
:SeeAlso:   advamation.h / advamation.c

:Author:    Advamation, Roland Koebler (support@advamation.de)
:Copyright: (c) Advamation (info@advamation.de)
:License:   open-source / MIT

:RCS:       $Id: AM_ERROR_Exception.java,v 1.4 2014/05/02 11:49:25 rk Exp $
***/

package am_rpi;

public class AM_ERROR_Exception extends Exception {
    public int code;
    public String message;

    public AM_ERROR_Exception(int code, String message) {
	super();
	this.message = message;
	this.code = code;
    }
    public String getMessage() {
	return this.message + " (" + this.code + ")";
    }
    public int getErrorCode() {
	return this.code;
    }
}

