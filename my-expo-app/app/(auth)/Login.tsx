import { useState } from 'react';
import { View, TextInput, TouchableOpacity, Image, ScrollView } from 'react-native';
import { useRouter } from 'expo-router';
import { Text } from '~/components/nativewindui/Text';
import { Ionicons } from '@expo/vector-icons';
import { useLogin } from '~/hooks/services/auth/useLogin';

export default function LoginScreen() {
  const router = useRouter();

  const login = useLogin();

  const [email, setEmail] = useState('admin@gmail.com');
  const [password, setPassword] = useState('Pass1234');
  const [secureText, setSecureText] = useState(true);

  const togglePassword = () => setSecureText(!secureText);

  const handleLogin = () => {
    if (email && password) {
      login.mutate({ email: email, password: password });
    } else {
      alert('لطفاً هر دو فیلد را پر کنید');
    }
  };

  return (
    <ScrollView className="flex-1">
      <View className="flex-1 items-center justify-center bg-background px-6 dark:bg-black">
        <Image
          source={require('../../assets/logo.webp')}
          className="mb-8 h-36 w-36 rounded-full"
          resizeMode="contain"
        />

        <Text variant="title1" className="mb-2 text-center font-semibold text-primary">
          خوش آمدید
        </Text>
        <Text variant="subhead" className="mb-8 text-center text-muted-foreground">
          لطفا برای ادامه وارد شوید
        </Text>

        <View className="flex w-full gap-4">
          <TextInput
            value={email}
            onChangeText={setEmail}
            placeholder="ایمیل"
            placeholderTextColor="#999"
            keyboardType="email-address"
            autoCapitalize="none"
            className="w-full rounded-xl border border-border bg-card px-4 py-3 text-right font-semibold text-foreground"
          />

          <View className="relative w-full">
            <TextInput
              value={password}
              onChangeText={setPassword}
              placeholder="رمز عبور"
              placeholderTextColor="#999"
              secureTextEntry={secureText}
              autoCapitalize="none"
              className="w-full rounded-xl border border-border bg-card px-4 py-3 pr-10 text-right font-semibold text-foreground"
            />
            <TouchableOpacity className="absolute right-3 top-3" onPress={togglePassword}>
              <Ionicons name={secureText ? 'eye-off' : 'eye'} size={20} color="#888" />
            </TouchableOpacity>
          </View>

          <TouchableOpacity className="self-end pt-1">
            <Text variant="footnote" className="text-primary">
              رمز عبور خود را فراموش کرده‌اید؟
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            onPress={handleLogin}
            className="rounded-xl bg-primary py-3 shadow-sm shadow-black/10 dark:shadow-none">
            <Text className="text-center font-medium text-white dark:text-black">ورود</Text>
          </TouchableOpacity>
        </View>

        <View className="mt-6 flex-row items-center gap-1">
          <Text variant="footnote" className="text-muted-foreground">
            حساب کاربری ندارید؟
          </Text>
          <TouchableOpacity onPress={() => router.push('/(auth)/Register')}>
            <Text variant="footnote" className="font-semibold text-primary">
              ثبت‌نام
            </Text>
          </TouchableOpacity>
        </View>
      </View>
    </ScrollView>
  );
}
