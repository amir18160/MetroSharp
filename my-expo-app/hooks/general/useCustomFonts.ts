import { useFonts } from 'expo-font';

export const useCustomFonts = () => {
  const [fontsLoaded, fontError] = useFonts({
    'IRANSans-Thin': require('../../assets/fonts/IRANSansX-ThinD4.ttf'),
    'IRANSans-UltraLight': require('../../assets/fonts/IRANSansX-UltraLightD4.ttf'),
    'IRANSans-Light': require('../../assets/fonts/IRANSansX-LightD4.ttf'),
    'IRANSans-Regular': require('../../assets/fonts/IRANSansX-RegularD4.ttf'),
    'IRANSans-Medium': require('../../assets/fonts/IRANSansX-MediumD4.ttf'),
    'IRANSans-DemiBold': require('../../assets/fonts/IRANSansX-DemiBoldD4.ttf'),
    'IRANSans-Bold': require('../../assets/fonts/IRANSansX-BoldD4.ttf'),
    'IRANSans-ExtraBold': require('../../assets/fonts/IRANSansX-ExtraBoldD4.ttf'),
    'IRANSans-Black': require('../../assets/fonts/IRANSansX-BlackD4.ttf'),
    'IRANSans-Heavy': require('../../assets/fonts/IRANSansX-HeavyD4.ttf'),
  });

  return { fontsLoaded, fontError };
};
