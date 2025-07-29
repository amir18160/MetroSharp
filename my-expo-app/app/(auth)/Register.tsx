import { useState } from 'react';
import { View, TextInput, TouchableOpacity, Image, Platform, StyleSheet } from 'react-native';
import { useRouter } from 'expo-router';
import { KeyboardAwareScrollView } from 'react-native-keyboard-aware-scroll-view';
import { Text } from '~/components/nativewindui/Text';
import { Ionicons } from '@expo/vector-icons';
import { Toast } from 'toastify-react-native'; // وارد کردن کامپوننت Toast
import { useRegister } from '~/hooks/services/auth/useRegister';

// این یک هوک فرضی برای مدیریت درخواست ثبت‌نام به سرور است
// می‌توانید منطق مشابه useLogin را برای این هوک نیز پیاده‌سازی کنید
// const { mutate: registerUser, isPending } = useRegister();

export default function RegisterScreen() {
  const router = useRouter();

  const [name, setName] = useState('');
  const [username, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [secureText, setSecureText] = useState(true);
  const [secureConfirmText, setSecureConfirmText] = useState(true);

  const register = useRegister();

  const validateAndRegister = () => {
    // ۱. بررسی خالی نبودن فیلدها
    if (!name || !email || !password || !username || !confirmPassword) {
      return Toast.show({
        type: 'error',
        text1: 'خطا',
        text2: 'پر کردن تمام فیلدها الزامی است.',
      });
    }

    // ۲. بررسی فرمت صحیح ایمیل
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      return Toast.show({
        type: 'error',
        text1: 'خطای ایمیل',
        text2: 'لطفاً یک ایمیل معتبر وارد کنید.',
      });
    }

    // ۳. بررسی پیچیدگی رمز عبور
    if (password.length < 8) {
      return Toast.show({ type: 'error', text1: 'رمز عبور باید حداقل ۸ کاراکتر باشد.' });
    }
    if (!/[a-z]/.test(password)) {
      return Toast.show({ type: 'error', text1: 'رمز عبور باید شامل حرف کوچک انگلیسی باشد.' });
    }
    if (!/[A-Z]/.test(password)) {
      return Toast.show({ type: 'error', text1: 'رمز عبور باید شامل حرف بزرگ انگلیسی باشد.' });
    }
    if (!/[0-9]/.test(password)) {
      return Toast.show({ type: 'error', text1: 'رمز عبور باید شامل حداقل یک عدد باشد.' });
    }

    // ۴. بررسی تطابق رمزهای عبور
    if (password !== confirmPassword) {
      return Toast.show({
        type: 'error',
        text1: 'خطا',
        text2: 'رمزهای عبور با یکدیگر مطابقت ندارند.',
      });
    }

    register.mutate({ email, name, password, username });
  };

  return (
    <KeyboardAwareScrollView
      enableOnAndroid
      extraScrollHeight={Platform.OS === 'ios' ? 60 : 100}
      keyboardShouldPersistTaps="handled"
      contentContainerStyle={styles.container}>
      <View className="items-center">
        <Image
          source={require('../../assets/logo.webp')}
          className="mb-6 h-36 w-36 rounded-full"
          resizeMode="contain"
        />
        <Text variant="title1" className="mb-2 text-center font-semibold text-primary">
          ایجاد حساب کاربری
        </Text>
        <Text variant="subhead" className="mb-8 text-center text-muted-foreground">
          برای شروع، ثبت‌نام کنید
        </Text>
      </View>

      <View className="flex gap-4">
        <TextInput
          value={name}
          onChangeText={setName}
          placeholder="نام و نام خانوادگی"
          placeholderTextColor="#999"
          className="w-full rounded-xl border border-border bg-card px-4 py-3 text-right font-semibold text-foreground"
        />

        <TextInput
          value={username}
          onChangeText={setUserName}
          placeholder="نام کاربری "
          placeholderTextColor="#999"
          className="w-full rounded-xl border border-border bg-card px-4 py-3 text-right font-semibold text-foreground"
        />

        <TextInput
          value={email}
          onChangeText={setEmail}
          placeholder="ایمیل"
          placeholderTextColor="#999"
          keyboardType="email-address"
          autoCapitalize="none"
          className="w-full rounded-xl border border-border bg-card px-4 py-3 text-right font-semibold text-foreground"
        />

        <View className="relative">
          <TextInput
            value={password}
            onChangeText={setPassword}
            placeholder="رمز عبور"
            placeholderTextColor="#999"
            secureTextEntry={secureText}
            className="w-full rounded-xl border border-border bg-card px-4 py-3 pr-10 text-right font-semibold text-foreground"
          />
          <TouchableOpacity
            className="absolute right-3 top-3"
            onPress={() => setSecureText(!secureText)}>
            <Ionicons name={secureText ? 'eye-off' : 'eye'} size={20} color="#888" />
          </TouchableOpacity>
        </View>

        <View className="relative">
          <TextInput
            value={confirmPassword}
            onChangeText={setConfirmPassword}
            placeholder="تکرار رمز عبور"
            placeholderTextColor="#999"
            secureTextEntry={secureConfirmText}
            className="w-full rounded-xl border border-border bg-card px-4 py-3 pr-10 text-right font-semibold text-foreground"
          />
          <TouchableOpacity
            className="absolute right-3 top-3"
            onPress={() => setSecureConfirmText(!secureConfirmText)}>
            <Ionicons name={secureConfirmText ? 'eye-off' : 'eye'} size={20} color="#888" />
          </TouchableOpacity>
        </View>

        <TouchableOpacity
          onPress={validateAndRegister}
          className="mt-2 rounded-xl bg-primary py-3 shadow-sm shadow-black/10 dark:shadow-none">
          <Text className="text-center font-medium text-white dark:text-black">ثبت‌نام</Text>
        </TouchableOpacity>
      </View>

      <View className="mt-6 flex-row items-center justify-center gap-1">
        <Text variant="footnote" className="text-muted-foreground">
          قبلاً حساب کاربری ساخته‌اید؟
        </Text>
        <TouchableOpacity onPress={() => router.push('/')}>
          <Text variant="footnote" className="font-semibold text-primary">
            ورود
          </Text>
        </TouchableOpacity>
      </View>
    </KeyboardAwareScrollView>
  );
}

const styles = StyleSheet.create({
  container: {
    flexGrow: 1,
    paddingHorizontal: 24,
    paddingTop: 48,
    backgroundColor: '#fff',
  },
});
