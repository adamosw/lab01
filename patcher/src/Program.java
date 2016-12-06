import java.io.File;
import java.io.FileInputStream;

import javax.swing.JFileChooser;


public class Program 
{
	public byte[] byteArray;
	int sectionHeaderAddress;
	int sectionEntrySize;
	int sectionEntryAmount;
	
	public static void main(String[] args) 
	{
		Program program = new Program();
		program.ReadFile();
		int sectionHeaderAddress = program.ReadBytes("20", 4);
		int sectionEntrySize = program.ReadBytes("2e", 2);
		int sectionEntryAmount = program.ReadBytes("30", 2);
		
		System.out.println(String.format("sectionHeaderAddress: %d", sectionHeaderAddress));
		System.out.println(String.format("sectionEntrySize: %d", sectionEntrySize));
		System.out.println(String.format("sectionEntryAmount: %d", sectionEntryAmount));
	}
	
	public void ReadFile()
	{
		final JFileChooser fc = new JFileChooser();
		int returnVal = fc.showOpenDialog(null);
		if (returnVal == JFileChooser.APPROVE_OPTION)
		{
			String path = fc.getSelectedFile().getAbsolutePath();
			File file = new File(path);
			byteArray = new byte[(int) file.length()];
			try 
			{
				FileInputStream fileInputStream = new FileInputStream(file);
				fileInputStream.read(byteArray);

			}
			catch (Exception ex)
			{
				System.out.println(ex.getMessage());
			}	
		}
	}
	
	public int ReadBytes(String index, int amount)
	{
		int start = Integer.parseInt(index, 16);	
		
		String result = "";
		for (int i = amount - 1; i >= 0; i--)
		{
			result = result + Integer.toHexString(byteArray[start + i]);
		}
		int intresult = Integer.parseInt(result, 16);
		
		return intresult;	
	}
}

