import axios from 'axios';
import { API_URL } from '~/constants/urls';

const apiClient = axios.create({
  baseURL: API_URL,
});

// Add a request interceptor
apiClient.interceptors.request.use(
  async (config) => {
    /*
    
    const token = await AsyncStorage.getItem('user-token');
    
    // If the token exists, add it to the Authorization header
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    */

    return config;
  },
  (error) => {
    // Handle request error
    return Promise.reject(error);
  }
);

export default apiClient;
