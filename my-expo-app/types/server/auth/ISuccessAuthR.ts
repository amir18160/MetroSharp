import { IResponse } from '~/types/generic/IResponse';
import { IAuthUserAccount } from '~/types/server/auth/IAuthUserAccount';

export interface ISuccessAuthR extends IResponse {
  data: IAuthUserAccount;
}
