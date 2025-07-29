import { useMutation } from '@tanstack/react-query';
import { login } from '~/services/remote/authService';
import { Toast } from 'toastify-react-native';
import { useRouter } from 'expo-router';
import { useUserStore } from '~/stores/userUserStore';

export const useLogin = () => {
  const router = useRouter();
  const setUser = useUserStore((store) => store.setUser);

  return useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      setUser(data.data);
      router.replace('/(drawer)/(user)/Home');

      Toast.show({
        type: 'success',
        text1: 'خوش برگشتید!  ورود با موفقیت انجام شد.',
      });
    },

    onError: (error) => {
      Toast.show({
        type: 'error',
        text1: 'ناموفق، لطفا نام کاربری و رمز عبور خود را بررسی کنید.',
      });
    },
  });
};
