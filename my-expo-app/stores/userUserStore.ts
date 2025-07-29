import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import { secureStorage } from '~/services/local/secureStorageAdaptor';
import { IAuthUserAccount } from '~/types/server/auth/IAuthUserAccount';

interface UserState {
  user: IAuthUserAccount | null;
  _hasHydrated: boolean;
}

interface UserActions {
  setUser: (user: IAuthUserAccount) => void;
  getUser: () => IAuthUserAccount | null;
  clearUser: () => void;
  setHasHydrated: (hydrated: boolean) => void;
}

export const useUserStore = create<UserState & UserActions>()(
  persist(
    (set, get) => ({
      user: null,
      _hasHydrated: false,

      setUser: (user) => {
        set({ user });
      },

      clearUser: () => {
        set({ user: null });
      },

      getUser: () => get().user,

      setHasHydrated: (hydrated: boolean) => {
        set({ _hasHydrated: hydrated });
      },
    }),
    {
      name: 'user-auth-storage',

      storage: createJSONStorage(() => secureStorage),
      onRehydrateStorage: () => (state, error) => {
        if (error) {
          console.log('Failed to rehydrate from secure storage:', error);
          state?.clearUser();
        } else {
          state?.setHasHydrated(true);
        }
      },
    }
  )
);
