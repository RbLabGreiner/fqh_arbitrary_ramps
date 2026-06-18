'Example to show how to read in an arbitrary 1D array from a .txt file using 'LoadArrayFromFile'

'===================
'=====Variables=====
start_var = 4
end_var = 0
ramp_time = 100
'=====Variables=====
'===================
	
'----------------------------------------------------- Constants -----------------------------------------------------------------------------
'Dim filePath As String = "Z:\\Timeline\\Analysis\\2024_03_11_read_array_example\\array_file.txt"

'Dim array_values As Double() = LoadArrayFromFile(filepath)
'Dim n_values As Double = array_values.GetUpperBound(0) 'gets the index of the last entry in the array. Note that since arrays are indexed from 0, the actual length of the array is (array_values + 1).
'Console.WriteLine("max array index: {0}", n_values)

'array_values(n_values) = 9 'replaces the value of the last index of the array

'Dim new_constant As Double = array_values(0) 'copies the last value of the array into a new constant
	
'----------------------------------------------------- Output -----------------------------------------------------------------------------

'quad grad
    'analogdata.AddRamp(array_values(index), array_values(index + 1), index * 42, (index + 1)*42, ps3_ao) 'ps8_ao
analogdata.AddRamp(start_var,end_var ,0,ramp_time ,ps3_ao)







