export interface IAuthUserAccount {
  id: string;
  userName: string;
  email: string;
  name: string;
  bio: string | null;
  telegramId: string | null;
  isConfirmed: boolean;
  createdAt: Date;
  updatedAt: Date;
  image: string | null;
  role: string;
  token: string;
}
