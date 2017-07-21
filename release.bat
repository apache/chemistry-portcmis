@echo off

rem
rem    Licensed to the Apache Software Foundation (ASF) under one
rem    or more contributor license agreements.  See the NOTICE file
rem    distributed with this work for additional information
rem    regarding copyright ownership.  The ASF licenses this file
rem    to you under the Apache License, Version 2.0 (the
rem    "License"); you may not use this file except in compliance
rem    with the License.  You may obtain a copy of the License at
rem
rem      http://www.apache.org/licenses/LICENSE-2.0
rem
rem    Unless required by applicable law or agreed to in writing,
rem    software distributed under the License is distributed on an
rem    "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
rem    KIND, either express or implied.  See the License for the
rem    specific language governing permissions and limitations
rem    under the License.
rem

rem This batch file creates a release.
rem It requires Cygwin.

set PORTCMISVERSION=0.3
set PORTCMISZIPSRC=chemistry-portcmis-%PORTCMISVERSION%-src.zip
set PORTCMISZIPBIN=chemistry-portcmis-%PORTCMISVERSION%-bin.zip
set PORTCMISZIPNUPKG=chemistry-portcmis-%PORTCMISVERSION%-nupkg.zip
set PORTCMISRC=RC1

set CYGWIN=ntea

echo Building...
cd PortCMIS
call build.bat
cd ..
cd PortCMISWin
call build.bat
cd ..

echo Creating release directories...
rmdir /S /Q release-src
mkdir release-src
rmdir /S /Q release-bin
mkdir release-bin
rmdir /S /Q release-nupkg
mkdir release-nupkg
rmdir /S /Q dist-dev
mkdir dist-dev
rmdir /S /Q publish
mkdir publish

echo Copying readme, etc...
copy LICENSE release-src
copy LICENSE release-bin
copy LICENSE release-nupkg
copy NOTICE release-src
copy NOTICE release-bin
copy NOTICE release-nupkg
copy DEPENDENCIES release-src
copy DEPENDENCIES release-bin
copy DEPENDENCIES release-nupkg
copy README release-src
copy README release-bin
copy README release-nupkg

echo Copying binaries ...
copy PortCMIS\bin\Release\PortCMIS.dll release-bin
copy PortCMIS\doc\PortCMIS.chm release-bin
copy PortCMISWin\bin\Release\PortCMISWin.dll release-bin
chmod -R a+rwx release-bin

echo Copying nupkg ...
xcopy PortCMIS\nupkg\*.nupkg release-nupkg
xcopy PortCMIS\doc\PortCMIS.chm release-nupkg
xcopy PortCMISWin\nupkg\*.nupkg release-nupkg
chmod -R a+rwx release-nupkg

echo Copying source...
mkdir release-src\src

mkdir release-src\src\PortCMIS
xcopy PortCMIS release-src\src\PortCMIS /E
del release-src\src\PortCMIS\project.lock.json
rmdir /S /Q release-src\src\PortCMIS\bin
rmdir /S /Q release-src\src\PortCMIS\obj
rmdir /S /Q release-src\src\PortCMIS\doc
rmdir /S /Q release-src\src\PortCMIS\nupkg

mkdir release-src\src\PortCMISWin
xcopy PortCMISWin release-src\src\PortCMISWin /E
rmdir /S /Q release-src\src\PortCMISWin\bin
rmdir /S /Q release-src\src\PortCMISWin\obj
rmdir /S /Q release-src\src\PortCMISWin\doc
rmdir /S /Q release-src\src\PortCMISWin\nupkg

chmod -R a+rwx release-src

echo Creating release file...
rmdir /S /Q artifacts
mkdir artifacts

cd release-src
zip -r  ../artifacts/%PORTCMISZIPSRC% *
cd ..

cd release-bin
zip -r  ../artifacts/%PORTCMISZIPBIN% *
cd ..

cd release-nupkg
zip -r  ../artifacts/%PORTCMISZIPNUPKG% *
cd ..

echo Signing release file...
cd artifacts

gpg --armor --output %PORTCMISZIPSRC%.asc --detach-sig %PORTCMISZIPSRC%
gpg --print-md MD5 %PORTCMISZIPSRC% > %PORTCMISZIPSRC%.md5
gpg --print-md SHA512 %PORTCMISZIPSRC% > %PORTCMISZIPSRC%.sha

gpg --armor --output %PORTCMISZIPBIN%.asc --detach-sig %PORTCMISZIPBIN%
gpg --print-md MD5 %PORTCMISZIPBIN% > %PORTCMISZIPBIN%.md5
gpg --print-md SHA512 %PORTCMISZIPBIN% > %PORTCMISZIPBIN%.sha

gpg --armor --output %PORTCMISZIPNUPKG%.asc --detach-sig %PORTCMISZIPNUPKG%
gpg --print-md MD5 %PORTCMISZIPNUPKG% > %PORTCMISZIPNUPKG%.md5
gpg --print-md SHA512 %PORTCMISZIPNUPKG% > %PORTCMISZIPNUPKG%.sha

cd ..

echo ========================================================================
echo.
echo Next steps:
echo -----------
echo.
echo - Check artifacts!!!
echo.
echo - Create RC tag:
echo   svn copy https://svn.apache.org/repos/asf/chemistry/portcmis/trunk https://svn.apache.org/repos/asf/chemistry/portcmis/tags/chemistry-portcmis-%PORTCMISVERSION%-%PORTCMISRC%
echo.
echo - Upload to dist/dev:
echo   cd dist-dev
echo   svn co https://dist.apache.org/repos/dist/dev/chemistry .
echo   mkdir chemistry-portcmis-%PORTCMISVERSION%-%PORTCMISRC%
echo   cd chemistry-portcmis-%PORTCMISVERSION%-%PORTCMISRC%
echo   copy ..\..\artifacts\* .
echo   svn add .
echo   svn commit -m "added PortCMIS %PORTCMISVERSION% artifacts"
echo   cd ..\..
echo.
echo - Send vote mail and wait 72 hours
echo.
echo - Upload to dist/release:
echo   cd publish
echo   svn co https://dist.apache.org/repos/dist/release/chemistry/portcmis .
echo   mkdir %PORTCMISVERSION%
echo   cd %PORTCMISVERSION%
echo   copy ..\..\artifacts\* .
echo   svn add .
echo   svn commit -m "added PortCMIS %PORTCMISVERSION% release to dist"
echo   cd ..\..
echo.
echo - Update website
echo.
echo - Close JIRA version and create a new one
echo.
echo - Rename tag:
echo   svn mv https://svn.apache.org/repos/asf/chemistry/portcmis/tags/chemistry-portcmis-%PORTCMISVERSION%-%PORTCMISRC% https://svn.apache.org/repos/asf/chemistry/portcmis/tags/chemistry-portcmis-%PORTCMISVERSION% -m 'renamed tag after successful release'
echo.
echo - Update DOAP file
echo.
echo - Send mail to email to announce@apache.org (with GPG signature)
echo.
echo - Remove previous versions from https://dist.apache.org/repos/dist/release/chemistry/portcmis
