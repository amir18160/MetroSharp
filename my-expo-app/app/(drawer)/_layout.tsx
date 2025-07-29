import { Drawer } from 'expo-router/drawer';

export default function DrawerLayout() {
  return (
    <Drawer>
      <Drawer.Screen name="user/home" options={{ title: 'User Home' }} />
      <Drawer.Screen name="admin/dashboard" options={{ title: 'Admin Dashboard' }} />
    </Drawer>
  );
}
