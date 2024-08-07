import axios from 'axios';

const axiosInstance = axios.create({
    baseURL: 'fabric:/Chat_Rooms',
    timeout: 1000,
    headers: {'X-Custom-Header': 'foobar'}
});

export default axiosInstance;