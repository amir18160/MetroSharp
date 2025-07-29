import apiClient from '~/api/apiClient';
import { ILogEntry, ILogList } from '~/types/server/logs/IServerLogs';

/*********** Get list of logs **************/

export async function getLogList() {
  const url = 'logs/get-logs';
  const response = await apiClient.get<ILogList>(url);
  return response.data;
}

/*********** Register **************/
export interface logDTO {
  filename: string;
}

export async function getLogJson(data: logDTO) {
  const url = `/logs/${data.filename}`;
  const response = await apiClient.get<ILogEntry>(url);
  return response.data;
}
