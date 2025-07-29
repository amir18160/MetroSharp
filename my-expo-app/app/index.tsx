import { useHeaderHeight } from '@react-navigation/elements';
import { LegendList } from '@legendapp/list';
import { cssInterop } from 'nativewind';
import * as React from 'react';
import { Linking, useWindowDimensions, View, Alert } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';

import { Icon } from '@roninoss/icons';

import { Text } from '~/components/nativewindui/Text';

import { useColorScheme } from '~/hooks/general/useColorScheme';
import { Button } from '~/components/Button';
import { useRouter } from 'expo-router';

cssInterop(LegendList, {
  className: 'style',
  contentContainerClassName: 'contentContainerStyle',
});

export default function Screen() {
  const router = useRouter();
  return (
    <>
      <Button title="Login" onPress={() => router.push('/(auth)/Login')} />
      <Button title="register" onPress={() => router.push('/(auth)/Register')} />
      <Button
        title="Open Drawer (User Home)"
        onPress={() => router.replace('/(drawer)/(admin)/dashboard')}
      />
    </>
  );
}
