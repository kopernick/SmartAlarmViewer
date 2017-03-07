Pass Multiple Command Parameter on ICommand
Bind multiple parameter on XAML

 <StackPanel Orientation="Horizontal">
        <PasswordBox Width="100" Height="30"  Name="txtPass"></PasswordBox>
            <PasswordBox Width="100" Height="30"  Name="txtPass1"></PasswordBox>
            <Button Height="30" Content="Click" Command="{Binding Cmd}" >
                <Button.CommandParameter>
                    <MultiBinding Converter="{StaticResource conv}">
                        <Binding ElementName="txtPass"></Binding>
                        <Binding ElementName="txtPass1"></Binding>
                    </MultiBinding>
                </Button.CommandParameter>
            </Button>
        </StackPanel>

Converter Class:


public class MyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Tuple<PasswordBox, PasswordBox> tuple = new Tuple<PasswordBox, PasswordBox>(
            (PasswordBox)values[0], (PasswordBox)values[1]);
            return (object)tuple;
                    
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


Create NameSpace for converter

<Window ....
        xmlns:con="clr-namespace:SampleMVVMApp.Converters">
 <Window.Resources>//Add converter into window resource
        <con:EmptyTextValidationConverter x:Key="EmptyTextValidation"></con:EmptyTextValidationConverter>
</Window.Resources>
..........
</Window>

Get all bounded parameter on Icommand Execute Method

Window.Resources

private void CmdExecute(object p)
        {
            var param = (Tuple<PasswordBox, PasswordBox>)p;
            string s=((PasswordBox)param.Item1).Password;
            string ss = ((PasswordBox)param.Item2).Password;
        }