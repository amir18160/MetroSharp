import React from 'react';
import { StyleSheet, View, Text, TouchableOpacity, I18nManager } from 'react-native';
import ToastManager from 'toastify-react-native';
import { Feather } from '@expo/vector-icons';

// --- Type Definitions ---
type ToastType = 'success' | 'error' | 'info' | 'warn';

interface CustomToastProps {
  text1: string;
  text2?: string;
  type: ToastType;
  onPress?: () => void;
  hide: () => void;
}

interface ToastDetails {
  icon: keyof typeof Feather.glyphMap;
  color: string;
  backgroundColor: string;
  borderColor: string;
}

// --- Custom Toast Component ---
const CustomToast: React.FC<CustomToastProps> = ({ text1, text2, type, onPress, hide }) => {
  const toastDetails: Record<ToastType, ToastDetails> = {
    success: {
      icon: 'check-circle',
      color: '#1eb95e',
      backgroundColor: '#E9F7EF',
      borderColor: '#A9DFBF',
    },
    error: {
      icon: 'x-circle',
      color: '#e74c3c',
      backgroundColor: '#FDEBE9',
      borderColor: '#F5B7B1',
    },
    info: {
      icon: 'info',
      color: '#3498db',
      backgroundColor: '#EBF5FB',
      borderColor: '#AED6F1',
    },
    warn: {
      icon: 'alert-triangle',
      color: '#f39c12',
      backgroundColor: '#FEF9E7',
      borderColor: '#FAD7A0',
    },
  };

  const details = toastDetails[type];

  return (
    <TouchableOpacity
      activeOpacity={1}
      onPress={onPress || hide}
      style={[
        styles.customToastContainer,
        {
          backgroundColor: details.backgroundColor,
          borderColor: details.borderColor,
          flexDirection: 'row-reverse',
        },
      ]}>
      <View style={styles.iconContainer}>
        <Feather name={details.icon} size={20} color={details.color} />
      </View>
      <View style={styles.textContainer}>
        <Text style={[styles.title, { color: details.color }]}>{text1}</Text>
        {text2 && <Text style={[styles.message, { textAlign: 'right' }]}>{text2}</Text>}
      </View>
      <TouchableOpacity onPress={hide} style={styles.closeButton}>
        <Feather name="x" size={20} color="#ccc9c9" />
      </TouchableOpacity>
    </TouchableOpacity>
  );
};

// --- Toast Configuration ---
const toastConfig = {
  success: (props: { text1: string; text2?: string; hide: () => void }) => (
    <CustomToast {...props} type="success" />
  ),
  error: (props: { text1: string; text2?: string; hide: () => void }) => (
    <CustomToast {...props} type="error" />
  ),
  info: (props: { text1: string; text2?: string; hide: () => void }) => (
    <CustomToast {...props} type="info" />
  ),
  warn: (props: { text1: string; text2?: string; hide: () => void }) => (
    <CustomToast {...props} type="warn" />
  ),
};

// --- Toast Provider ---
export const ToastProvider: React.FC = () => {
  return (
    <ToastManager
      config={toastConfig}
      width="90%"
      height={85}
      topOffset={50}
      duration={4000}
      isRTL={I18nManager.isRTL}
    />
  );
};

// --- Styles ---
const styles = StyleSheet.create({
  customToastContainer: {
    width: '95%',
    alignItems: 'center',
    paddingVertical: 12,
    paddingHorizontal: 15,
    borderRadius: 12,
    borderWidth: 1,
    elevation: 8,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.1,
    shadowRadius: 8,
  },
  iconContainer: {
    marginHorizontal: 10,
  },
  textContainer: {
    flex: 1,
    justifyContent: 'center',
  },
  title: {
    fontSize: 14,
    textAlign: 'right',
    fontFamily: 'IRANSans-DemiBold',
  },
  message: {
    fontSize: 13,
    color: '#555',
    textAlign: 'right',
    fontFamily: 'IRANSans-DemiBold',
    marginTop: 3,
  },
  closeButton: {
    padding: 5,
    marginHorizontal: 10,
  },
});
