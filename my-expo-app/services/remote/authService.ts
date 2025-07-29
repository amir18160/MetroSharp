import apiClient from '~/api/apiClient';
import { ISuccessAuthR } from '~/types/server/auth/ISuccessAuthR';

/*********** Login **************/
export interface LoginDTO {
  email: string;
  password: string;
}

export async function login(data: LoginDTO) {
  const url = 'account/login';
  const response = await apiClient.post<ISuccessAuthR>(url, data);
  return response.data;
}

/*********** Register **************/
export interface RegisterDTO {
  name: string;
  username: string;
  email: string;
  password: string;
}

export async function register(data: RegisterDTO) {
  const url = 'account/register';
  const response = await apiClient.post<ISuccessAuthR>(url, data);
  return response.data;
}
