import * as signalR from '@microsoft/signalr';

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private connectionId: string | null = null;
    private groups: Set<string> = new Set();

    public async startConnection(url: string): Promise<void> {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(url)
            .configureLogging(signalR.LogLevel.Information)
            .build();

        this.connection.onreconnected(() => {
            this.connectionId = this.connection!.connectionId;
            for (const group in this.groups) {
                this.connection!.invoke('JoinGroup', group);
            }
        })

        try {
            await this.connection.start();
            console.log('SignalR Connected');
            this.connectionId = this.connection.connectionId;
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
        }
    }

    public getConnectionId(): string | null {
        return this.connectionId;
    }

    public getConnection(): signalR.HubConnection | null {
        return this.connection;
    }

    public joinGroup(groupName: string): void {
        if (this.connection) {
            this.groups.add(groupName);
        }
    } 

    public leaveGroup(groupName: string): void {
        if (this.connection) {
            this.groups.delete(groupName);
        }
    }
}

export const signalRService = new SignalRService();