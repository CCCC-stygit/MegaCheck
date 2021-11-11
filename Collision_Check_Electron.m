clear
f = 40; %mm
Coll_Ang = 0; %degrees
Couch_Ang = 0; %degrees
Gantry_Ang = 0; %degrees

% Convert to rads
Coll_Ang_Rad = Coll_Ang*pi/180;
Couch_Ang_Rad = Couch_Ang*pi/180;
Gantry_Ang_Rad = Gantry_Ang*pi/180;

% Make Raster Points
App_Size = 150; %mm
Margin = 40; %mm
Step = 5;%mm
temp = (-App_Size/2 - Margin):Step:(App_Size/2 + Margin);
[MX,MY] = meshgrid(temp,temp);
[x_size, y_size] = size(MX);


% Compute P
k = 0;
for i = 1:y_size
    for j = 1:x_size
       k = k+1;
       A = cos(Gantry_Ang_Rad)*(MX(i,j)*cos(Coll_Ang_Rad) + MY(i,j)*sin(Coll_Ang_Rad)) - f*sin(Gantry_Ang_Rad);
       B = - MX(i,j)*sin(Coll_Ang_Rad) + MY(i,j)*cos(Coll_Ang_Rad);
       P_x(k) = cos(Couch_Ang_Rad)*A + sin(Couch_Ang_Rad)*B;
       P_y(k) = sin(Gantry_Ang_Rad)*(MX(i,j)*cos(Coll_Ang_Rad) + MY(i,j)*sin(Coll_Ang_Rad)) - f*cos(Gantry_Ang_Rad);
       P_z(k) = -sin(Couch_Ang_Rad)*A + cos(Couch_Ang_Rad)*B;
    end
end

% Plot
%L = length(p_x);
figure
plot3(P_x,P_y,P_z,'.k')
xlabel('X (mm)')
ylabel('Y (mm)')
zlabel('Z (mm)')
hold on
plot3(P_x(1),P_y(1),P_z(1),'or')
