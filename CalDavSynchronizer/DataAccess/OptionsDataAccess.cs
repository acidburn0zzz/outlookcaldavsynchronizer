﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using Microsoft.Win32;

namespace CalDavSynchronizer.DataAccess
{
  public class OptionsDataAccess : IOptionsDataAccess
  {
    private readonly string _optionsFilePath;
    private const string s_OptionsRegistryKey = @"Software\CalDavSynchronizer";

    public OptionsDataAccess (string optionsFilePath)
    {
      _optionsFilePath = optionsFilePath;
    }

    public Options[] LoadOptions ()
    {
      if (!File.Exists (_optionsFilePath))
        return new Options[] { };
      else
        return Serializer<Options[]>.Deserialize (File.ReadAllText (_optionsFilePath));
    }

    public void SaveOptions (Options[] options)
    {
      if (!Directory.Exists (Path.GetDirectoryName (_optionsFilePath)))
        Directory.CreateDirectory (Path.GetDirectoryName (_optionsFilePath));

      File.WriteAllText (_optionsFilePath, Serializer<Options[]>.Serialize (options));
    }

    public bool ShouldCheckForNewerVersions
    {
      get
      {
        using (var key = OpenOptionsKey())
        {
          return (int) (key.GetValue ("CheckForNewerVersions") ?? 1) != 0;
        }
      }
      set
      {
        using (var key = OpenOptionsKey())
        {
          key.SetValue ("CheckForNewerVersions", value ? 1 : 0);
        }
      }
    }

    public Version IgnoreUpdatesTilVersion
    {
      get
      {
        using (var key = OpenOptionsKey())
        {
          var versionString = (string) key.GetValue ("IgnoreUpdatesTilVersion");
          if (!string.IsNullOrEmpty (versionString))
            return new Version (versionString);
          else
            return null;
        }
      }
      set
      {
        using (var key = OpenOptionsKey())
        {
          if (value != null)
            key.SetValue ("IgnoreUpdatesTilVersion", value.ToString());
          else
            key.DeleteValue ("IgnoreUpdatesTilVersion");
        }
      }
    }

    private static RegistryKey OpenOptionsKey ()
    {
      var key = Registry.CurrentUser.OpenSubKey (s_OptionsRegistryKey, true);
      if (key == null)
        key = Registry.CurrentUser.CreateSubKey (s_OptionsRegistryKey);
      return key;
    }
  }
}